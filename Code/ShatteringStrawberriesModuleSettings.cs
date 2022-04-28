namespace Celeste.Mod.ShatteringStrawberries {
    [SettingName("modoptions_ShatteringStrawberries")]
    public class ShatteringStrawberriesModuleSettings : EverestModuleSettings {
        [SettingName("modoptions_ShatteringStrawberries_Enabled")]
        [SettingSubText("modoptions_ShatteringStrawberries_Enabled_desc")]
        public bool Enabled { get; set; } = true;

        [SettingName("modoptions_ShatteringStrawberries_Shards")]
        [SettingSubText("modoptions_ShatteringStrawberries_Shards_desc")]
        public bool Shards { get; set; } = true;

        [SettingName("modoptions_ShatteringStrawberries_Juice")]
        [SettingSubText("modoptions_ShatteringStrawberries_Juice_desc")]
        public bool Juice { get; set; } = true;
    }
}
