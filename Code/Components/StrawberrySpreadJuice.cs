using Celeste.Mod.ShatteringStrawberries.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Components {
    public class StrawberrySpreadJuice : Component {
        private static MTexture[] juiceTextures;

        private readonly Vector2 offset;

        private float extend;
        public readonly short Orientation;

        private readonly MTexture fullTexture, texture;

        private Vector2 scale = new(1);
        private readonly float rotation;

        private readonly Color color;

        public StrawberrySpreadJuice(StrawberryDebris debris, Platform platform, short orientation = 0)
            : base(active: true, visible: true) {
            Orientation = orientation;
            rotation = -MathHelper.PiOver2 * Orientation;

            if (orientation == 0)
                offset = new Vector2(debris.Center.X, debris.Bottom);
            else
                offset = new Vector2(orientation < 0 ? debris.Left : debris.Right, debris.Center.Y);

            offset -= platform.Position;

            texture = (fullTexture = Calc.Random.Choose(juiceTextures)).GetSubtexture(0, 0, 0, 0);
            color = debris.JuiceColor;
        }

        public void Extend(float amount) {
            extend += amount;

            float sign = Math.Sign(extend);
            scale.X = sign;
            if (Orientation != 0)
                scale.X *= -Orientation;

            float length = Calc.Clamp(Math.Abs(extend) - 3, 1, fullTexture.Width);
            fullTexture.GetSubtexture(0, 0, (int)length, 5, applyTo: texture);
        }

        public override void Render()
            => texture?.Draw(Entity.Position + offset, Vector2.Zero, color, scale, rotation);

        internal static void InitializeContent() {
            juiceTextures = GFX.Game.GetAtlasSubtextures("ShatteringStrawberries/juice/").ToArray();
        }
    }
}
