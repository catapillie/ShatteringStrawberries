using Celeste.Mod.ShatteringStrawberries.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Components {
    public class StrawberrySpreadJuice : Component {
        private readonly StrawberryDebris debris;

        private float from, to;
        private readonly float y;

        public StrawberrySpreadJuice(StrawberryDebris debris)
            : base(active: true, visible: true) {
            this.debris = debris;
            from = to = debris.CenterX;
            y = debris.Bottom;
        }

        public void Extend() {
            to = debris.CenterX; 
        }

        public override void Render() {
            float d = to - from;
            float l = Math.Abs(d);
            int sign = Math.Sign(d);
            Vector2 scale = new(sign, 1);

            GFX.Game["ShatteringStrawberries/juice/strawberry"]
                .GetSubtexture(0, 0, Calc.Clamp((int)l - 3, 1, 64), 5)
                .Draw(new(from, y), Vector2.Zero, Color.White, scale);

        }
    }
}
