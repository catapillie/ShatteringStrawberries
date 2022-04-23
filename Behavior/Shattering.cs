using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ShatteringStrawberries {
    public static class Shattering {
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

        public static void OnShatter(this Strawberry strawberry) {
            DynData<Strawberry> data = new(strawberry);

            bool ghost = (bool)data["isGhostBerry"];

            ParticleType explodeParticle;
            if (strawberry.Golden)
                explodeParticle = ghost ? P_GhostStrawberryExplode : P_GoldStrawberryExplode;
            else if (strawberry.Moon)
                explodeParticle = ghost ? P_GhostMoonStrawberryExplode : P_MoonStrawberryExplode;
            else
                explodeParticle = ghost ? P_GhostStrawberryExplode : P_RedStrawberryExplode;

                Level level = strawberry.SceneAs<Level>();
            level.Shake();
            level.Displacement.AddBurst(strawberry.Position, 0.4f, 12f, 36f, 0.5f);
            for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f) {
                Vector2 position = strawberry.Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(4, 10));
                level.Particles.Emit(explodeParticle, position, num);
            }

            Audio.Play(SFX.game_10_puffer_splode, strawberry.Position);
        }

        internal static void InitializeContent() {
            P_RedStrawberryExplode = new ParticleType(Seeker.P_Regen) {
                Color = Strawberry.P_Glow.Color2,
                Color2 = RedStrawberryColor,
            };

            P_GhostStrawberryExplode = new ParticleType(Seeker.P_Regen) {
                Color2 = GhostStrawberryColor,
            };

            P_GoldStrawberryExplode = new ParticleType(Seeker.P_Regen) {
                Color2 = GoldStrawberryColor,
            };

            P_MoonStrawberryExplode = new ParticleType(Seeker.P_Regen) {
                Color = Strawberry.P_Glow.Color2,
                Color2 = MoonStrawberryColor,
            };

            P_GhostMoonStrawberryExplode = new ParticleType(Seeker.P_Regen) {
                Color2 = GhostMoonStrawberryColor,
            };
        }
    }
}
