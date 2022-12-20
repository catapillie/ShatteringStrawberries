using Celeste.Mod.ShatteringStrawberries.Components;
using Celeste.Mod.ShatteringStrawberries.Settings;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Entities;

[Pooled]
public class ExplosionDebris : Actor
{
    public const float LifetimeMin = 4f;
    public const float LifetimeMax = 8f;
    public const float BlastAngleRange = 2.35619449019f;
    public const float MaxFallSpeed = 280f;
    public const float Gravity = 220f;
    public const float AirFriction = 5f;
    public const float GroundFriction = 75f;

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
    private bool outline;

    private LiquidSetting liquidMode;
    private bool SpreadsLiquid => liquidMode != LiquidSetting.None;
    private DebrisLiquid groundLiquid, leftWallLiquid, rightWallLiquid;
    private Platform groundPlatform, leftWallPlatform, rightWallPlatform;
    public Color LiquidColor { get; private set; }

    // this entity is handled in a pool, it must have a parameterless ctor
    // so this is only called once per debris, and when they are removed from the scene, they can be reused,
    // in which case, we have to rely on StrawberryDebris.Init (down below)
    public ExplosionDebris()
        : base(Vector2.Zero)
    {
        Collider = new Hitbox(4, 4, -2, -2);

        Add(sfx);
        sfx.Pause();
        eventInstance = (EventInstance)new DynData<SoundSource>(sfx)["instance"];
    }

    // we initialize our entity, knowing that it might've previously been instantiated.
    public ExplosionDebris Init(Vector2 position, MTexture texture, LiquidSetting liquidMode, Color liquidColor, bool outline = false)
    {
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
        this.liquidMode = liquidMode;
        LiquidColor = liquidColor;

        lifeTimer = Calc.Random.Range(LifetimeMin, LifetimeMax);
        alpha = 1f;

        DismissLiquid();

        this.outline = outline;

        return this;
    }

    private void OnCollideH(CollisionData data)
    {
        if (speed.X != 0)
        {
            int sign = Math.Sign(speed.X);
            if (sign > 0)
                TryCreateRightWallSpreadLiquid();
            else
                TryCreateLeftWallSpreadLiquid();
        }

        speed.X = 0f;
        rotationVel = 0f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (speed.Y > 0)
        {
            ImpactSfx(speed.Y);
            hitGround = true;

            TryCreateGroundSpreadLiquid();
        }

        speed.Y = 0f;
        rotationVel = 0f;
    }

    protected override void OnSquish(CollisionData data)
    {
        base.OnSquish(data);
        RemoveSelf();
    }

    private void TryCreateGroundSpreadLiquid()
    {
        if (SpreadsLiquid)
        {
            Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitY);
            if (platform != null && groundPlatform != platform)
            {
                groundLiquid?.Dismiss();
                Scene.Add(groundLiquid = new DebrisLiquid(this, platform, liquidMode));
                groundPlatform = platform;
            }
        }
    }

    private void TryCreateLeftWallSpreadLiquid()
    {
        if (SpreadsLiquid)
        {
            Platform platform = CollideFirstOutside<Platform>(Position - Vector2.UnitX);
            if (platform != null && leftWallPlatform != platform)
            {
                leftWallLiquid?.Dismiss();
                Scene.Add(leftWallLiquid = new DebrisLiquid(this, platform, liquidMode, -1));
                leftWallPlatform = platform;
            }
        }
    }

    private void TryCreateRightWallSpreadLiquid()
    {
        if (SpreadsLiquid)
        {
            Platform platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
            if (platform != null && rightWallPlatform != platform)
            {
                Scene.Add(rightWallLiquid = new DebrisLiquid(this, platform, liquidMode, 1));
                rightWallPlatform = platform;
            }
        }
    }

    private void DismissLiquid()
    {
        groundLiquid?.Dismiss(); leftWallLiquid?.Dismiss(); rightWallLiquid?.Dismiss();
        groundLiquid = leftWallLiquid = rightWallLiquid = null;
        groundPlatform = leftWallPlatform = rightWallPlatform = null;
    }

    private void ImpactSfx(float speed)
        => Audio.Play(SFX.game_gen_debris_dirt, Position, "debris_velocity", Calc.ClampedMap(speed, 0f, 150f));

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = Scene as Level;
    }

    private bool OutsideBounds()
        => Top >= level.Bounds.Bottom + 5
        || Bottom <= level.Bounds.Top - 5
        || Left >= level.Bounds.Right + 5
        || Right <= level.Bounds.Left - 5;

    public override void Update()
    {
        Collidable = true;

        bool onGround = OnGround();

        float oldYSpeed = speed.Y;

        float friction = onGround ? GroundFriction : AirFriction;
        speed.X = Calc.Approach(speed.X, 0, friction * Engine.DeltaTime);
        if (!onGround)
            speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, Gravity * Engine.DeltaTime);

        if (speed != Vector2.Zero)
        {
            if (speed.Y * oldYSpeed < 0)
                DismissLiquid(); // Our Y speed has changed sign, so let's dismiss the current spread liquid entity

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

            if (isCurrentlyOnGround)
            {
                if (speed.Y == 0 && speed.X != 0)
                {
                    if (doParticles)
                        level.Particles.Emit(ParticleTypes.Dust, new Vector2(CenterX, Bottom), Color.White);

                    sliding = true;
                    slideAmount = Math.Abs(speed.X);

                    TryCreateGroundSpreadLiquid();
                    groundLiquid?.Extend(dx);
                }
            }
            else
            {
                if (speed.Y != 0 && speed.X == 0)
                {
                    Platform platform = null;

                    platform = CollideFirstOutside<Platform>(Position - Vector2.UnitX);
                    if (platform != null)
                    {
                        sliding = true;

                        if (doParticles)
                            level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Left, CenterY), Color.White);

                        TryCreateLeftWallSpreadLiquid();
                        leftWallLiquid?.Extend(dy);
                    }

                    platform = CollideFirstOutside<Platform>(Position + Vector2.UnitX);
                    if (platform != null)
                    {
                        sliding = true;

                        if (doParticles)
                            level.ParticlesFG.Emit(ParticleTypes.Dust, new Vector2(Right, CenterY), Color.White);

                        TryCreateRightWallSpreadLiquid();
                        rightWallLiquid?.Extend(dy);
                    }

                    slideAmount = Math.Abs(speed.Y);
                }
            }

            rotation += rotationVel * Engine.DeltaTime;

            if (sliding)
            {
                if (!sfx.Playing)
                    eventInstance.setVolume(Calc.Clamp(slideAmount / 24f, 0, 2.25f));
            }
            else
            {
                if (sfx.Playing)
                    sfx.Pause();
                DismissLiquid();
            }

            if (OutsideBounds())
                RemoveSelf();
        }
        else
        {
            if (sfx.Playing)
                sfx.Pause();
        }

        if (previousLiftSpeed != Vector2.Zero && LiftSpeed == Vector2.Zero)
            speed += previousLiftSpeed;
        previousLiftSpeed = LiftSpeed;

        if (lifeTimer > 0f)
            lifeTimer -= Engine.DeltaTime;
        else if (alpha > 0f)
        {
            alpha -= Engine.DeltaTime;
            if (alpha <= 0f)
                RemoveSelf();
        }

        Collidable = false;
    }

    public override void Render()
    {
        if (outline)
            Texture.DrawOutlineCentered(Center, Color.White * alpha, 1f, rotation);
        else
            Texture.DrawCentered(Center, Color.White * alpha, 1f, rotation);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);

        sfx.Stop();
        DismissLiquid();
    }
}
