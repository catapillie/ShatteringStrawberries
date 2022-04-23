using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ShatteringStrawberries {
    public class ShatteringStrawberriesModule : EverestModule {
        public static ShatteringStrawberriesModule Instance { get; private set; }

        public override Type SettingsType => typeof(ShatteringStrawberriesModuleSettings);
        public static ShatteringStrawberriesModuleSettings Settings => (ShatteringStrawberriesModuleSettings) Instance._Settings;

        public ShatteringStrawberriesModule() {
            Instance = this;
        }

        public override void Load() {
            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            // TODO: unapply any hooks applied in Load()
        }
    }
}