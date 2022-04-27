using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Entities {
    public class StrawberryDebris : Actor {
        public static MTexture[] Shards_Strawberry { get; private set; }

        public const float BlastAngleRange = 2.35619449019f;
        public const float MaxFallSpeed = 280f;
        public const float Gravity = 220f;
        public const float AirFriction = 5f;
        public const float GroundFriction = 45f;

        private float lifeTime;

        private Vector2 speed;
        private float rotation = Calc.Random.NextFloat(MathHelper.TwoPi);
        private float rotationVel = Calc.Random.Range(-6f, 6f);

        private bool hitGround;

        private readonly SoundSource sfx = new(SFX.char_mad_wallslide);
        private readonly EventInstance eventInstance;

        private Level level;

        private readonly MTexture shard = Calc.Random.Choose(Shards_Strawberry);

        public StrawberryDebris(Strawberry strawberry)
            : base(strawberry.Position) {
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
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            level = Scene as Level;
        }

        public override void Update() {
            base.Update();

            bool onGround = OnGround();

            float friction = onGround ? GroundFriction : AirFriction;
            speed.X = Calc.Approach(speed.X, 0, friction * Engine.DeltaTime);
            if (!onGround)
                speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);

            hitGround = false;
            MoveH(speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(speed.Y * Engine.DeltaTime, OnCollideV);

            bool sliding = false;
            float slideAmount = 0f;
            bool isCurrentlyOnGround = hitGround || onGround;
            if (isCurrentlyOnGround && speed.Y == 0 && speed.X != 0) {
                level.Particles.Emit(ParticleTypes.Dust, new Vector2(CenterX, Bottom), Color.White);
                sliding = true;
                slideAmount = Math.Abs(speed.X);
            } else if (!isCurrentlyOnGround && speed.Y != 0 && speed.X == 0) {
                if (CollideCheck<Solid>(Position + new Vector2(-1, 0))) {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Left, CenterY), Color.White);
                    sliding = true;
                }
                if (CollideCheck<Solid>(Position + new Vector2(1, 0))) {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Right, CenterY), Color.White);
                    sliding = true;
                }
                slideAmount = Math.Abs(speed.Y);
            }

            rotation += rotationVel * Engine.DeltaTime;

            if (sfx.Playing)
                eventInstance.setVolume(Calc.Clamp(slideAmount / 24f, 0, 2.25f));

            if (sliding && !sfx.Playing)
                sfx.Resume();
            else if (!sliding && sfx.Playing)
                sfx.Pause();

            lifeTime += Engine.DeltaTime;
        }

        private void OnCollideH(CollisionData data) {
            speed.X = 0f;
            rotationVel = 0f;
        }

        private void OnCollideV(CollisionData data) {
            if (speed.Y > 0) {
                ImpactSfx(speed.Y);
                hitGround = true;
            }

            speed.Y = 0f;
            rotationVel = 0f;
        }

        private void ImpactSfx(float speed)
            => Audio.Play(SFX.game_gen_debris_dirt, Position, "debris_velocity", Calc.ClampedMap(speed, 0f, 150f));

        public override void Render() {
            base.Render();

            shard.DrawCentered(Center, Color.White, 1f, rotation);
        }

        internal static void InitializeContent() {
            Shards_Strawberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards_strawberry/").ToArray();
        }
    }
}
