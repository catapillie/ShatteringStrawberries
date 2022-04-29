using Celeste.Mod.ShatteringStrawberries.Components;
using Celeste.Mod.ShatteringStrawberries.Settings;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Entities {
    [Pooled]
    public class StrawberryDebris : Actor {
        public const float LifetimeMin = 4f;
        public const float LifetimeMax = 8f;
        public const float BlastAngleRange = 2.35619449019f;
        public const float MaxFallSpeed = 280f;
        public const float Gravity = 220f;
        public const float AirFriction = 5f;
        public const float GroundFriction = 75f;

        public static MTexture[] Shards_Strawberry { get; private set; }
        public static MTexture[] Shards_Ghostberry { get; private set; }
        public static MTexture[] Shards_Goldenberry { get; private set; }
        public static MTexture[] Shards_Goldghostberry { get; private set; }
        public static MTexture[] Shards_Moonberry { get; private set; }
        public static MTexture[] Shards_Ghostmoonberry { get; private set; }

        private float lifeTimer;

        private Vector2 speed;
        private Vector2 previousLiftSpeed;

        private float rotation = Calc.Random.NextFloat(MathHelper.TwoPi);
        private float rotationVel = Calc.Random.Range(-6f, 6f);

        private bool hitGround;

        private readonly SoundSource sfx = new(SFX.char_mad_wallslide);
        private readonly EventInstance eventInstance;

        private Level level;

        public MTexture Texture { get; private set; }
        private float alpha;

        private bool spreadsJuice;
        private StrawberrySpreadJuice groundJuice, leftWallJuice, rightWallJuice;
        private Platform groundPlatform, leftWallPlatform, rightWallPlatform;
        public Color JuiceColor { get; private set; }

        // this entity is handled in a pool, it must have a parameterless ctor
        // so this is only called once per debris, and when they are removed from the scene, they can be reused,
        // in which case, we have to rely on StrawberryDebris.Init (down below)
        public StrawberryDebris()
            : base(Vector2.Zero) {
            Collider = new Hitbox(4, 4, -2, -2);

            Add(sfx);
            sfx.Pause();
            eventInstance = (EventInstance)new DynData<SoundSource>(sfx)["instance"];
        }

        // we initialize our entity, knowing that it might've previously been instantiated.
        public StrawberryDebris Init(Vector2 position, MTexture texture, Color juiceColor) {
            Position = position;

            float angle = Calc.Random.Range(-BlastAngleRange, BlastAngleRange) - MathHelper.PiOver2;
            float mag = Calc.Random.Range(80, 160);

            previousLiftSpeed = Vector2.Zero;
            speed = Calc.AngleToVector(angle, mag);
            speed.X *= 1.2f;
            if (speed.Y < 0)
                speed.Y *= 1.3f;

            Collidable = false;

            Texture = texture;
            spreadsJuice = ShatteringStrawberriesModule.Settings.Juice != JuiceSetting.None;
            JuiceColor = juiceColor;

            lifeTimer = Calc.Random.Range(LifetimeMin, LifetimeMax);
            alpha = 1f;

            DismissJuice();

            return this;
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

        protected override void OnSquish(CollisionData data) {
            base.OnSquish(data);
            RemoveSelf();
        }

        private void TryCreateGroundSpreadJuice() {
            if (spreadsJuice) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitY);
                if (platform != null && groundPlatform != platform) {
                    groundJuice?.Dismiss();
                    Scene.Add(groundJuice = new StrawberrySpreadJuice(this, platform));
                    groundPlatform = platform;
                }
            }
        }

        private void TryCreateLeftWallSpreadJuice() {
            if (spreadsJuice) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                if (platform != null && leftWallPlatform != platform) {
                    leftWallJuice?.Dismiss();
                    Scene.Add(leftWallJuice = new StrawberrySpreadJuice(this, platform, 1));
                    leftWallPlatform = platform;
                }
            }
        }

        private void TryCreateRightWallSpreadJuice() {
            if (spreadsJuice) {
                Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                if (platform != null && rightWallPlatform != platform) {
                    rightWallJuice?.Dismiss();
                    Scene.Add(rightWallJuice = new StrawberrySpreadJuice(this, platform, 1));
                    rightWallPlatform = platform;
                }
            }
        }

        private void DismissJuice() {
            groundJuice?.Dismiss(); leftWallJuice?.Dismiss(); rightWallJuice?.Dismiss();
            groundJuice = leftWallJuice = rightWallJuice = null;
            groundPlatform = leftWallPlatform = rightWallPlatform = null;
        }

        private void ImpactSfx(float speed)
            => Audio.Play(SFX.game_gen_debris_dirt, Position, "debris_velocity", Calc.ClampedMap(speed, 0f, 150f));

        public override void Awake(Scene scene) {
            base.Awake(scene);
            level = Scene as Level;
        }

        private bool OutsideBounds() 
            => Top >= level.Bounds.Bottom + 5
            || Bottom <= level.Bounds.Top - 5
            || Left >= level.Bounds.Right + 5
            || Right <= level.Bounds.Left - 5;

        public override void Update() {
            Collidable = true;

            bool onGround = OnGround();

            float oldYSpeed = speed.Y;

            float friction = onGround ? GroundFriction : AirFriction;
            speed.X = Calc.Approach(speed.X, 0, friction * Engine.DeltaTime);
            if (!onGround)
                speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);

            if (speed != Vector2.Zero) {
                if (speed.Y * oldYSpeed < 0)
                    DismissJuice(); // Our Y speed has changed sign, so let's dismiss the current spread juice component

                Vector2 oldPos = Position;

                hitGround = false;
                MoveH(speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(speed.Y * Engine.DeltaTime, OnCollideV);

                bool doParticles = Scene.OnInterval(0.035f);

                bool sliding = false;
                float slideAmount = 0f;
                bool isCurrentlyOnGround = hitGround || onGround;

                float dx = X - oldPos.X;
                float dy = Y - oldPos.Y;

                if (isCurrentlyOnGround) {
                    if (speed.Y == 0 && speed.X != 0) {
                        if (doParticles)
                            level.Particles.Emit(ParticleTypes.Dust, new Vector2(CenterX, Bottom), Color.White);

                        sliding = true;
                        slideAmount = Math.Abs(speed.X);

                        TryCreateGroundSpreadJuice();
                        groundJuice?.Extend(dx);
                    }
                } else {
                    if (speed.Y != 0 && speed.X == 0) {
                        Platform platform = null;

                        platform = CollideFirstOutside<Platform>(Position - Vector2.UnitX);
                        if (platform != null) {
                            sliding = true;

                            if (doParticles)
                                level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Left, CenterY), Color.White);

                            TryCreateLeftWallSpreadJuice();
                            leftWallJuice?.Extend(dy);
                        }

                        platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                        if (platform != null) {
                            sliding = true;

                            if (doParticles)
                                level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Right, CenterY), Color.White);

                            TryCreateRightWallSpreadJuice();
                            rightWallJuice?.Extend(dy);
                        }

                        slideAmount = Math.Abs(speed.Y);
                    }
                }

                rotation += rotationVel * Engine.DeltaTime;

                if (sliding) {
                    if (!sfx.Playing)
                        eventInstance.setVolume(Calc.Clamp(slideAmount / 24f, 0, 2.25f));
                } else {
                    if (sfx.Playing)
                        sfx.Pause();
                    DismissJuice();
                }

                if (OutsideBounds())
                    RemoveSelf();
            } else {
                if (sfx.Playing)
                    sfx.Pause();
            }

            if (previousLiftSpeed != Vector2.Zero && LiftSpeed == Vector2.Zero)
                speed += previousLiftSpeed;
            previousLiftSpeed = LiftSpeed;

            if (lifeTimer > 0f)
                lifeTimer -= Engine.DeltaTime;
            else if (alpha > 0f) {
                alpha -= Engine.DeltaTime;
                if (alpha <= 0f)
                    RemoveSelf();
            }

            Collidable = false;
        }

        public override void Render()
            => Texture.DrawCentered(Center, Color.White * alpha, 1f, rotation);

        public override void Removed(Scene scene) {
            base.Removed(scene);

            sfx.Pause();
            DismissJuice();
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
