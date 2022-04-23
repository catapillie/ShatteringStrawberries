using System;

namespace Celeste.Mod.ShatteringStrawberries {
    public class ShatteringStrawberriesModule : EverestModule {
        public static ShatteringStrawberriesModule Instance { get; private set; }

        public override Type SettingsType => typeof(ShatteringStrawberriesModuleSettings);
        public static ShatteringStrawberriesModuleSettings Settings => (ShatteringStrawberriesModuleSettings) Instance._Settings;

        public ShatteringStrawberriesModule() {
            Instance = this;
        }

        public override void Load() {
            Hooks.Hook();
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            Shattering.InitializeContent();
        }

        public override void Unload() {
            Hooks.Unhook();
        }
    }
}