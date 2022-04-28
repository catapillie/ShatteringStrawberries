using Celeste.Mod.ShatteringStrawberries.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Entities {
    public class StrawberryDebris : Actor {
        public static MTexture[] Shards_Strawberry { get; private set; }
        public static MTexture[] Shards_Ghostberry { get; private set; }
        public static MTexture[] Shards_Goldenberry { get; private set; }
        public static MTexture[] Shards_Goldghostberry { get; private set; }
        public static MTexture[] Shards_Moonberry { get; private set; }
        public static MTexture[] Shards_Ghostmoonberry { get; private set; }

        public const float BlastAngleRange = 2.35619449019f;
        public const float MaxFallSpeed = 280f;
        public const float Gravity = 220f;
        public const float AirFriction = 5f;
        public const float GroundFriction = 75f;

        private float lifeTime;

        private Vector2 speed;
        private Vector2 previousLiftSpeed;

        private float rotation = Calc.Random.NextFloat(MathHelper.TwoPi);
        private float rotationVel = Calc.Random.Range(-6f, 6f);

        private bool hitGround;

        private readonly SoundSource sfx = new(SFX.char_mad_wallslide);
        private readonly EventInstance eventInstance;

        private Level level;

        private readonly MTexture shard;

        private StrawberrySpreadJuice groundJuice, leftWallJuice, rightWallJuice;
        public readonly Color JuiceColor;

        public StrawberryDebris(Strawberry strawberry, MTexture texture, Color color)
            : base(Calc.Floor(strawberry.Position)) {
            Collider = new Hitbox(4, 4, -2, -2);

            float angle = Calc.Random.Range(-BlastAngleRange, BlastAngleRange) - MathHelper.PiOver2;
            float mag = Calc.Random.Range(80, 160);

            speed = Calc.AngleToVector(angle, mag);
            speed.X *= 1.2f;
            if (speed.Y < 0)
                speed.Y *= 1.3f;

            Add(sfx);
            sfx.Pause();

            eventInstance = (EventInstance)new DynData<SoundSource>(sfx)["instance"];
            eventInstance.setVolume(2f);

            BloomPoint bloom = strawberry.Get<BloomPoint>();
            Add(new BloomPoint(bloom.Alpha / 2f, bloom.Radius / 2f));

            shard = texture;
            JuiceColor = color;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            level = Scene as Level;
        }

        public override void Update() {
            base.Update();

            bool onGround = OnGround();

            float oldYSpeed = speed.Y;

            float friction = onGround ? GroundFriction : AirFriction;
            speed.X = Calc.Approach(speed.X, 0, friction * Engine.DeltaTime);
            if (!onGround)
                speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);

            // Our Y speed has changed sign, so let's dismiss the current spread juice component
            if (speed.Y * oldYSpeed < 0)
                DismissJuice();

            Vector2 oldPos = Position;

            hitGround = false;
            MoveH(speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(speed.Y * Engine.DeltaTime, OnCollideV);

            bool doParticles = Scene.OnInterval(0.02f);

            bool sliding = false;
            float slideAmount = 0f;
            bool isCurrentlyOnGround = hitGround || onGround;

            float dx = X - oldPos.X;
            float dy = Y - oldPos.Y;

            if (isCurrentlyOnGround && speed.Y == 0 && speed.X != 0) {
                if (doParticles)
                    level.Particles.Emit(ParticleTypes.Dust, new Vector2(CenterX, Bottom), Color.White);

                sliding = true;
                slideAmount = Math.Abs(speed.X);

                TryCreateGroundSpreadJuice();
                groundJuice?.Extend(dx);
            } else if (!isCurrentlyOnGround && speed.Y != 0 && speed.X == 0) {
                Platform platform = null;

                if ((platform = CollideFirstOutside<Platform>(Position - Vector2.UnitX)) != null) {
                    if (doParticles)
                        level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Left, CenterY), Color.White);

                    sliding = true;

                    if (leftWallJuice == null)
                        platform.Add(leftWallJuice = new StrawberrySpreadJuice(this, platform, -1));
                    leftWallJuice?.Extend(dy);
                }

                if ((platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX)) != null) {
                    if (doParticles)
                        level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Right, CenterY), Color.White);

                    sliding = true;

                    if (rightWallJuice == null)
                        platform.Add(rightWallJuice = new StrawberrySpreadJuice(this, platform, 1));
                    rightWallJuice?.Extend(dy);
                }
                slideAmount = Math.Abs(speed.Y);
            }

            rotation += rotationVel * Engine.DeltaTime;

            if (sliding)
                eventInstance.setVolume(Calc.Clamp(slideAmount / 24f, 0, 2.25f));
            else
                DismissJuice();

            if (sliding && !sfx.Playing)
                sfx.Resume();
            else if (!sliding && sfx.Playing)
                sfx.Pause();

            if (previousLiftSpeed != Vector2.Zero && LiftSpeed == Vector2.Zero)
                speed += previousLiftSpeed;
            previousLiftSpeed = LiftSpeed;

            lifeTime += Engine.DeltaTime;
        }

        private void OnCollideH(CollisionData data) {
            if (speed.X != 0) {
                int sign = Math.Sign(speed.X);
                if (sign > 0)
                    TryCreateRightWallSpreadJuice();
                else
                    TryCreateLeftWallSpreadJuice();
            }

            speed.X = 0f;
            rotationVel = 0f;
        }

        private void OnCollideV(CollisionData data) {
            if (speed.Y > 0) {
                ImpactSfx(speed.Y);
                hitGround = true;

                TryCreateGroundSpreadJuice();
            }

            speed.Y = 0f;
            rotationVel = 0f;
        }

        private void TryCreateGroundSpreadJuice() {
            if (groundJuice == null) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitY);
                if (platform != null)
                    platform.Add(groundJuice = new StrawberrySpreadJuice(this, platform));
            }
        }

        private void TryCreateLeftWallSpreadJuice() {
            if (leftWallJuice == null) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                if (platform != null)
                    platform.Add(leftWallJuice = new StrawberrySpreadJuice(this, platform, 1));
            }
        }

        private void TryCreateRightWallSpreadJuice() {
            if (rightWallJuice == null) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                if (platform != null)
                    platform.Add(rightWallJuice = new StrawberrySpreadJuice(this, platform, 1));
            }
        }

        private void DismissJuice()
            => groundJuice = leftWallJuice = rightWallJuice = null;

        private void ImpactSfx(float speed)
            => Audio.Play(SFX.game_gen_debris_dirt, Position, "debris_velocity", Calc.ClampedMap(speed, 0f, 150f));

        public override void Render() {
            base.Render();

            shard.DrawCentered(Center, Color.White, 1f, rotation);
        }

        internal static void InitializeContent() {
            Shards_Strawberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/strawberry/").ToArray();
            Shards_Ghostberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/ghostberry/").ToArray();
            Shards_Goldenberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/goldenberry/").ToArray();
            Shards_Goldghostberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/goldghostberry/").ToArray();
            Shards_Moonberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/moonberry/").ToArray();
            Shards_Ghostmoonberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/ghostmoonberry/").ToArray();
        }
    }
}
