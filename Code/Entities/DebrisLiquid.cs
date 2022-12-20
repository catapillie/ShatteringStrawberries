using Celeste.Mod.ShatteringStrawberries.Entities;
using Celeste.Mod.ShatteringStrawberries.Settings;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Components;

public class DebrisLiquid : Entity
{
    private static MTexture[] liquidTextures;

    private readonly Vector2 offset;

    private float extend;
    public readonly short Orientation;

    private readonly MTexture fullTexture, texture;

    private Vector2 scale = new(1);
    private readonly float rotation;

    private readonly Color color;

    private readonly Platform platform;

    private float lifeTimer = 3f, alpha = 1f;

    private readonly LiquidSetting mode;

    public DebrisLiquid(ExplosionDebris debris, Platform platform, LiquidSetting mode, short orientation = 0)
        : base(platform.Position)
    {
        Tag = Tags.Global;

        this.platform = platform;

        Depth = platform.Depth - 1;

        Orientation = orientation;
        rotation = -MathHelper.PiOver2 * Orientation;

        if (orientation == 0)
            offset = new Vector2(debris.Center.X, debris.Bottom);
        else
            offset = new Vector2(orientation < 0 ? debris.Left : debris.Right, debris.Center.Y);

        offset -= platform.Position;

        texture = (fullTexture = Calc.Random.Choose(liquidTextures)).GetSubtexture(0, 0, 0, 0);
        color = debris.LiquidColor;

        this.mode = mode;
    }

    public void Extend(float amount)
    {
        extend += amount;

        float sign = Math.Sign(extend);
        scale.X = sign;
        if (Orientation != 0)
            scale.X *= -Orientation;

        float length = Calc.Clamp(Math.Abs(extend) - 3, 1, fullTexture.Width);
        fullTexture.GetSubtexture(0, 0, (int)length, 5, applyTo: texture);
    }

    public void Dismiss()
    {
        if (Math.Abs(extend) <= 3)
            RemoveSelf();
    }

    public override void Update()
    {
        base.Update();
        if (platform.Scene == null)
            RemoveSelf();
        else if (mode == LiquidSetting.Fading)
        {
            if (lifeTimer > 0f)
                lifeTimer -= Engine.DeltaTime;
            else if (alpha > 0f)
            {
                alpha -= Engine.DeltaTime / 2f;
                if (alpha <= 0f)
                    RemoveSelf();
            }
        }
    }

    public override void Render()
        => texture?.Draw(platform.Position + offset, Vector2.Zero, color * alpha, scale, rotation);

    internal static void InitializeContent()
    {
        liquidTextures = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/juice/").ToArray();
    }
}
