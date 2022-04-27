using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Entities {
    public class StrawberryDebris : Actor {
        public const float BlastAngleRange = 2.35619449019f;
        public const float MaxFallSpeed = 280f;
        public const float Gravity = 220f;
        public const float AirFriction = 5f;
        public const float GroundFriction = 45f;

        private float lifeTime;

        public Vector2 Speed;
        private bool hitGround;

        private readonly SoundSource sfx = new(SFX.char_mad_wallslide);
        private readonly EventInstance eventInstance;

        private Level level;

        public StrawberryDebris(Vector2 position)
            : base(position) {
            Collider = new Hitbox(4, 4, -2, -2);

            float angle = Calc.Random.Range(-BlastAngleRange, BlastAngleRange) - MathHelper.PiOver2;
            float mag = Calc.Random.Range(80, 160);

            Speed = Calc.AngleToVector(angle, mag);
            Speed.X *= 1.2f;
            if (Speed.Y < 0)
                Speed.Y *= 1.3f;

            Add(sfx);
            sfx.Pause();

            eventInstance = (EventInstance)new DynData<SoundSource>(sfx)["instance"];
            eventInstance.setVolume(2f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            level = Scene as Level;
        }

        public override void Update() {
            base.Update();

            bool onGround = OnGround();

            float friction = onGround ? GroundFriction : AirFriction;
            Speed.X = Calc.Approach(Speed.X, 0, friction * Engine.DeltaTime);
            if (!onGround)
                Speed.Y = Calc.Approach(Speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);

            hitGround = false;
            MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);

            bool sliding = false;
            float slideAmount = 0f;
            bool isCurrentlyOnGround = hitGround || onGround;
            if (isCurrentlyOnGround && Speed.Y == 0 && Speed.X != 0) {
                level.Particles.Emit(ParticleTypes.Dust, new Vector2(CenterX, Bottom), Color.White);
                sliding = true;
                slideAmount = Math.Abs(Speed.X);
            } else if (!isCurrentlyOnGround && Speed.Y != 0 && Speed.X == 0) {
                if (CollideCheck<Solid>(Position + new Vector2(-1, 0))) {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Left, CenterY), Color.White);
                    sliding = true;
                }
                if (CollideCheck<Solid>(Position + new Vector2(1, 0))) {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Right, CenterY), Color.White);
                    sliding = true;
                }
                slideAmount = Math.Abs(Speed.Y);
            }

            if (sfx.Playing)
                eventInstance.setVolume(Calc.Clamp(slideAmount / 24f, 0, 2.25f));

            if (sliding && !sfx.Playing)
                sfx.Resume();
            else if (!sliding && sfx.Playing)
                sfx.Pause();

            lifeTime += Engine.DeltaTime;
        }

        private void OnCollideH(CollisionData data) {
            Speed.X = 0;
        }

        private void OnCollideV(CollisionData data) {
            if (Speed.Y > 0) {
                ImpactSfx(Speed.Y);
                hitGround = true;
            }

            Speed.Y = 0;
        }

        private void ImpactSfx(float speed)
            => Audio.Play(SFX.game_gen_debris_dirt, Position, "debris_velocity", Calc.ClampedMap(speed, 0f, 150f));

        public override void Render() {
            base.Render();

            Draw.HollowRect(Collider, Vector2.Zero == Speed ? Color.Red : Color.Blue);
        }
    }
}
