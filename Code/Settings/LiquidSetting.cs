namespace Celeste.Mod.ShatteringStrawberries.Settings;

public enum LiquidSetting
{
    None, Fading, Permanent,
}

public static class JuiceSettingExt
{
    public static string Name(this LiquidSetting setting)
        => Dialog.Clean($"settings_ShatteringStrawberries_Juice_{setting}_name");

    public static string Info(this LiquidSetting setting)
        => Dialog.Clean($"settings_ShatteringStrawberries_Juice_{setting}_info");
}
