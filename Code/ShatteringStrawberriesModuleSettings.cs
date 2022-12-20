using Celeste.Mod.ShatteringStrawberries.Settings;
using Celeste.Mod.ShatteringStrawberries.UI;
using System;
using System.Linq;

namespace Celeste.Mod.ShatteringStrawberries;
[SettingName("modoptions_ShatteringStrawberries")]
public class ShatteringStrawberriesModuleSettings : EverestModuleSettings
{
    [SettingName("modoptions_ShatteringStrawberries_Enabled")]
    [SettingSubText("modoptions_ShatteringStrawberries_Enabled_desc")]
    public bool Enabled { get; set; } = true;

    //[SettingName("modoptions_ShatteringStrawberries_Shards")]
    //[SettingSubText("modoptions_ShatteringStrawberries_Shards_desc")]
    public ShardsSetting Shards { get; set; } = ShardsSetting.Few;
    public void CreateShardsEntry(TextMenu menu, bool _)
    {
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
    public LiquidSetting Juice { get; set; } = LiquidSetting.None;
    public void CreateJuiceEntry(TextMenu menu, bool _)
    {
        TextMenuExt.EaseInSubHeaderExt info = null;

        var item = new TextMenuExt.EnumSlider<LiquidSetting>(Dialog.Clean("modoptions_ShatteringStrawberries_Juice"), Juice)
            .Change(setting => info.Title = (Juice = setting).Info());
        item.Values = (Enum.GetValues(typeof(LiquidSetting)) as LiquidSetting[])
            .Select(setting => Tuple.Create(setting.Name(), setting))
            .ToList();

        menu.Add(item);

        item.AddWarning(menu, "         " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_b"), icon: false);
        item.AddWarning(menu, "    " + Dialog.Clean("modoptions_ShatteringStrawberries_Juice_warn_a"), icon: true);
        info = item.AddThenGetDescription(menu, Juice.Info());
        item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_Juice_desc"));
    }

    [SettingName("modoptions_ShatteringStrawberries_PlayerExplosion")]
    [SettingSubText("modoptions_ShatteringStrawberries_PlayerExplosion_desc")]
    public bool PlayerExplosion { get; set; } = false;

    //[SettingName("modoptions_ShatteringStrawberries_PlayerDebris")]
    //[SettingSubText("modoptions_ShatteringStrawberries_PlayerDebris_desc")]
    public ShardsSetting PlayerDebris { get; set; } = ShardsSetting.Few;
    public void CreatePlayerDebrisEntry(TextMenu menu, bool _)
    {
        TextMenuExt.EaseInSubHeaderExt info = null;

        var item = new TextMenuExt.EnumSlider<ShardsSetting>(Dialog.Clean("modoptions_ShatteringStrawberries_PlayerDebris"), Shards)
            .Change(setting => info.Title = (Shards = setting).Info());
        item.Values = (Enum.GetValues(typeof(ShardsSetting)) as ShardsSetting[])
            .Select(setting => Tuple.Create(setting.Name(), setting))
            .ToList();

        menu.Add(item);

        info = item.AddThenGetDescription(menu, Shards.Info());
        item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_PlayerDebris_desc"));
    }

    //[SettingName("modoptions_ShatteringStrawberries_PlayerBlood")]
    //[SettingSubText("modoptions_ShatteringStrawberries_PlayerBlood_desc")]
    public LiquidSetting PlayerBlood { get; set; } = LiquidSetting.None;
    public void CreatePlayerBloodEntry(TextMenu menu, bool _)
    {
        TextMenuExt.EaseInSubHeaderExt info = null;

        var item = new TextMenuExt.EnumSlider<LiquidSetting>(Dialog.Clean("modoptions_ShatteringStrawberries_PlayerBlood"), Juice)
            .Change(setting => info.Title = (Juice = setting).Info());
        item.Values = (Enum.GetValues(typeof(LiquidSetting)) as LiquidSetting[])
            .Select(setting => Tuple.Create(setting.Name(), setting))
            .ToList();

        menu.Add(item);

        item.AddWarning(menu, "         " + Dialog.Clean("modoptions_ShatteringStrawberries_PlayerBlood_warn_b"), icon: false);
        item.AddWarning(menu, "    " + Dialog.Clean("modoptions_ShatteringStrawberries_PlayerBlood_warn_a"), icon: true);
        info = item.AddThenGetDescription(menu, Juice.Info());
        item.AddDescription(menu, Dialog.Clean("modoptions_ShatteringStrawberries_PlayerBlood_desc"));
    }
}
