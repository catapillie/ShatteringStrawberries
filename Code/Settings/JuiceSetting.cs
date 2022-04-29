namespace Celeste.Mod.ShatteringStrawberries.Settings {
    public enum JuiceSetting {
        None, Fading, Permanent
    }

    public static class JuiceSettingExt {
        public static string Name(this JuiceSetting setting)
            => Dialog.Clean($"settings_ShatteringStrawberries_Juice_{setting}_name");

        public static string Info(this JuiceSetting setting)
            => Dialog.Clean($"settings_ShatteringStrawberries_Juice_{setting}_info");
    }
}
