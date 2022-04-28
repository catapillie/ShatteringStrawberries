using Celeste.Mod.ShatteringStrawberries.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
        public bool Juice { get; set; } = false;
        public void CreateJuiceEntry(TextMenu menu, bool _) {
            var item = new TextMenu.OnOff(Dialog.Clean("modoptions_ShatteringStrawberries_Juice"), Juice)
                .Change(value => Juice = value);

            menu.Add(item);

            item.AddWarning(menu, "         " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_b"), icon: false);
            item.AddWarning(menu, "    " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_a"), icon: true);
            item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_Juice_desc"));
        }
    }
}
