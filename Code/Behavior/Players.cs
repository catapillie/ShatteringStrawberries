using Celeste.Mod.ShatteringStrawberries.Entities;
using Celeste.Mod.ShatteringStrawberries.Settings;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries;

public static class Players
{
    public static Color Blood { get; } = Calc.HexToColor("880808");
    public static ParticleType P_PlayerExplode { get; private set; }

    public static void OnShatter(PlayerDeadBody body)
    {
        Calc.PushRandom();

        Level level = body.SceneAs<Level>();
        level.Shake(0.4f);
        level.Displacement.AddBurst(body.Center, 0.4f, 12f, 36f, 0.5f);

        for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = body.Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(4, 10));
            level.Particles.Emit(P_PlayerExplode, position, num);
        }

        Player player = new DynData<PlayerDeadBody>(body).Get<Player>("player");

        LiquidSetting blood = ShatteringStrawberriesModule.Settings.PlayerBlood;
        int amount = ShatteringStrawberriesModule.Settings.PlayerDebris.Amount();

        if (amount > 0)
        {
            MTexture full = player.Sprite.Animations["idle"].Frames[0];

            int mx = full.Width / 2 - 2;
            int my = full.Height - 4;

            Vector2 from = Calc.Floor(body.Center);
            for (int i = 0; i < amount; ++i)
            {
                int ox = Calc.Random.Next(-4, 5);
                int oy = Calc.Random.Next(0, 20);

                ExplosionDebris debris = Engine.Pooler.Create<ExplosionDebris>();
                level.Add(debris.Init(from, full.GetSubtexture(mx + ox, my - oy, 4, 4), blood, Blood));
            }
        }

        Audio.Play(SFX.game_10_puffer_splode, body.Center);

        Calc.PopRandom();
    }

    internal static void InitializeContent()
    {
        P_PlayerExplode = new ParticleType(Seeker.P_Regen)
        {
            Color = Strawberry.P_Glow.Color2,
            Color2 = Blood,
        };
    }
}
