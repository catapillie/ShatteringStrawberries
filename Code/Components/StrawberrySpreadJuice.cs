using Celeste.Mod.ShatteringStrawberries.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ShatteringStrawberries.Components {
    public class StrawberrySpreadJuice : Component {
        private readonly Vector2 offset;

        private Vector2 scale = new(1);
        private float extend;
        private MTexture texture;

        public StrawberrySpreadJuice(StrawberryDebris debris, Platform platform)
            : base(active: true, visible: true) {
            offset = new Vector2(debris.Center.X, debris.Bottom) - platform.Position;
        }

        public void Extend(float amount) {
            //to = debris.CenterX;
            extend += amount;

            scale.X = Math.Sign(extend);
            float length = Calc.Clamp(Math.Abs(extend) - 3, 1, 64);
            texture = GFX.Game["ShatteringStrawberries/juice/strawberry"]
                .GetSubtexture(0, 0, (int)length, 5);
            Console.WriteLine(length);
        }

        public override void Render() 
            => texture?.Draw(Entity.Position + offset, Vector2.Zero, Color.White, scale);
    }
}
