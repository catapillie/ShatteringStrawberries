namespace Celeste.Mod.ShatteringStrawberries.Settings;

public enum ShardsSetting
{
    None, VeryFew, Few, Many, TooMuch, WayTooMuch,
}

public static class ShardsSettingExt
{
    public static string Name(this ShardsSetting setting)
        => Dialog.Clean($"settings_ShatteringStrawberries_Shards_{setting}_name");

    public static string Info(this ShardsSetting setting)
        => Dialog.Clean($"settings_ShatteringStrawberries_Shards_{setting}_info");

    public static int Amount(this ShardsSetting setting)
        => setting switch
        {
            ShardsSetting.WayTooMuch => 64,
            ShardsSetting.TooMuch => 24,
            ShardsSetting.Many => 16,
            ShardsSetting.Few => 8,
            ShardsSetting.VeryFew => 4,
            _ => 0,
        };
}
