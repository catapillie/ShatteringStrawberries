using Celeste.Mod.ShatteringStrawberries.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Components {
    public class StrawberrySpreadJuice : Component {
        private readonly Vector2 offset;

        private Vector2 scale = new(1);
        private readonly float rotation;
        private float extend;
        private MTexture texture;

        public readonly short Orientation;

        public StrawberrySpreadJuice(StrawberryDebris debris, Platform platform, short orientation = 0)
            : base(active: true, visible: true) {
            Orientation = orientation;
            rotation = -MathHelper.PiOver2 * Orientation;

            if (orientation == 0)
                offset = new Vector2(debris.Center.X, debris.Bottom);
            else
                offset = new Vector2(orientation < 0 ? debris.Left : debris.Right, debris.Center.Y);

            offset -= platform.Position;
        }


        public void Extend(float amount) {
            extend += amount;

            float sign = Math.Sign(extend);
            scale.X = sign;
            if (Orientation != 0)
                scale.X *= -Orientation;

            float length = Calc.Clamp(Math.Abs(extend) - 3, 1, 64);
            texture = GFX.Game["ShatteringStrawberries/juice/strawberry"]
                .GetSubtexture(0, 0, (int)length, 5);
        }

        public override void Render()
            => texture?.Draw(Entity.Position + offset, Vector2.Zero, Color.White * 0.6f, scale, rotation);
    }
}
