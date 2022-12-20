using Celeste.Mod.ShatteringStrawberries.Entities;
using Celeste.Mod.ShatteringStrawberries.Settings;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries;

public static class Strawberries
{
    public static readonly Color RedStrawberryColor = Calc.HexToColor("de2a2a");
    public static readonly Color GhostStrawberryColor = Calc.HexToColor("8290c6");
    public static readonly Color GoldStrawberryColor = Calc.HexToColor("dec02a");
    public static readonly Color MoonStrawberryColor = Calc.HexToColor("5cff6f");
    public static readonly Color GhostMoonStrawberryColor = Calc.HexToColor("5ccbff");

    public static ParticleType P_RedStrawberryExplode { get; private set; }
    public static ParticleType P_GhostStrawberryExplode { get; private set; }
    public static ParticleType P_GoldStrawberryExplode { get; private set; }
    public static ParticleType P_MoonStrawberryExplode { get; private set; }
    public static ParticleType P_GhostMoonStrawberryExplode { get; private set; }

    public static MTexture[] Shards_Strawberry { get; private set; }
    public static MTexture[] Shards_Ghostberry { get; private set; }
    public static MTexture[] Shards_Goldenberry { get; private set; }
    public static MTexture[] Shards_Goldghostberry { get; private set; }
    public static MTexture[] Shards_Moonberry { get; private set; }
    public static MTexture[] Shards_Ghostmoonberry { get; private set; }

    public static void OnShatter(this Strawberry strawberry)
    {
        if (!ShatteringStrawberriesModule.Settings.Enabled)
            return;

        Calc.PushRandom();

        DynData<Strawberry> data = new(strawberry);

        bool ghost = (bool)data["isGhostBerry"];

        ParticleType explodeParticle;
        MTexture[] shards;
        Color color;
        if (strawberry.Golden)
        {
            if (ghost)
            {
                explodeParticle = P_GhostStrawberryExplode;
                shards = Shards_Goldghostberry;
                color = GhostStrawberryColor;
            }
            else
            {
                explodeParticle = P_GoldStrawberryExplode;
                shards = Shards_Goldenberry;
                color = GoldStrawberryColor;
            }
        }
        else if (strawberry.Moon)
        {
            if (ghost)
            {
                explodeParticle = P_GhostMoonStrawberryExplode;
                shards = Shards_Ghostmoonberry;
                color = GhostMoonStrawberryColor;
            }
            else
            {
                explodeParticle = P_MoonStrawberryExplode;
                shards = Shards_Moonberry;
                color = MoonStrawberryColor;
            }
        }
        else
        {
            if (ghost)
            {
                explodeParticle = P_GhostStrawberryExplode;
                shards = Shards_Ghostberry;
                color = GhostStrawberryColor;
            }
            else
            {
                explodeParticle = P_RedStrawberryExplode;
                shards = Shards_Strawberry;
                color = RedStrawberryColor;
            }
        }

        Level level = strawberry.SceneAs<Level>();
        level.Shake();
        level.Displacement.AddBurst(strawberry.Position, 0.4f, 12f, 36f, 0.5f);
        for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = strawberry.Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(4, 10));
            level.Particles.Emit(explodeParticle, position, num);
        }

        LiquidSetting juice = ShatteringStrawberriesModule.Settings.Juice;
        int amount = ShatteringStrawberriesModule.Settings.Shards.Amount();
        if (amount > 0)
        {
            color *= 0.75f;
            Vector2 from = Calc.Floor(strawberry.Position);
            for (int i = 0; i < amount; ++i)
            {
                MTexture texture = Calc.Random.Choose(shards);
                ExplosionDebris debris = Engine.Pooler.Create<ExplosionDebris>();
                level.Add(debris.Init(from, texture, juice, color));
            }
        }

        Audio.Play(SFX.game_10_puffer_splode, strawberry.Position);

        Calc.PopRandom();
    }

    internal static void InitializeContent()
    {
        P_RedStrawberryExplode = new ParticleType(Seeker.P_Regen)
        {
            Color = Strawberry.P_Glow.Color2,
            Color2 = RedStrawberryColor,
        };

        P_GhostStrawberryExplode = new ParticleType(Seeker.P_Regen)
        {
            Color2 = GhostStrawberryColor,
        };

        P_GoldStrawberryExplode = new ParticleType(Seeker.P_Regen)
        {
            Color2 = GoldStrawberryColor,
        };

        P_MoonStrawberryExplode = new ParticleType(Seeker.P_Regen)
        {
            Color = Strawberry.P_Glow.Color2,
            Color2 = MoonStrawberryColor,
        };

        P_GhostMoonStrawberryExplode = new ParticleType(Seeker.P_Regen)
        {
            Color2 = GhostMoonStrawberryColor,
        };

        Shards_Strawberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/strawberry/").ToArray();
        Shards_Ghostberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/ghostberry/").ToArray();
        Shards_Goldenberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/goldenberry/").ToArray();
        Shards_Goldghostberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/goldghostberry/").ToArray();
        Shards_Moonberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/moonberry/").ToArray();
        Shards_Ghostmoonberry = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/shards/ghostmoonberry/").ToArray();
    }
}
