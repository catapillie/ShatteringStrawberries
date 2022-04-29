using Celeste.Mod.ShatteringStrawberries.Settings;
using Celeste.Mod.ShatteringStrawberries.UI;
using System;
using System.Linq;

namespace Celeste.Mod.ShatteringStrawberries {
    [SettingName("modoptions_ShatteringStrawberries")]
    public class ShatteringStrawberriesModuleSettings : EverestModuleSettings {
        [SettingName("modoptions_ShatteringStrawberries_Enabled")]
        [SettingSubText("modoptions_ShatteringStrawberries_Enabled_desc")]
        public bool Enabled { get; set; } = true;

        //[SettingName("modoptions_ShatteringStrawberries_Shards")]
        //[SettingSubText("modoptions_ShatteringStrawberries_Shards_desc")]
        public ShardsSetting Shards { get; set; } = ShardsSetting.Few;
        public void CreateShardsEntry(TextMenu menu, bool _) {
            TextMenuExt.EaseInSubHeaderExt info = null;

            var item = new TextMenuExt.EnumSlider<ShardsSetting>(Dialog.Clean("modoptions_ShatteringStrawberries_Shards"), Shards)
                .Change(setting => info.Title = (Shards = setting).Info());
            item.Values = (Enum.GetValues(typeof(ShardsSetting)) as ShardsSetting[])
                .Select(setting => Tuple.Create(setting.Name(), setting))
                .ToList();

            menu.Add(item);

            info = item.AddThenGetDescription(menu, Shards.Info());
            item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_Shards_desc"));
        }

        //[SettingName("modoptions_ShatteringStrawberries_Juice")]
        //[SettingSubText("modoptions_ShatteringStrawberries_Juice_desc")]
        public JuiceSetting Juice { get; set; } = JuiceSetting.None;
        public void CreateJuiceEntry(TextMenu menu, bool _) {
            TextMenuExt.EaseInSubHeaderExt info = null;

            var item = new TextMenuExt.EnumSlider<JuiceSetting>(Dialog.Clean("modoptions_ShatteringStrawberries_Juice"), Juice)
                .Change(setting => info.Title = (Juice = setting).Info());
            item.Values = (Enum.GetValues(typeof(JuiceSetting)) as JuiceSetting[])
                .Select(setting => Tuple.Create(setting.Name(), setting))
                .ToList();

            menu.Add(item);

            item.AddWarning(menu, "         " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_b"), icon: false);
            item.AddWarning(menu, "    " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_a"), icon: true);
            info = item.AddThenGetDescription(menu, Juice.Info());
            item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_Juice_desc"));
        }
    }
}
