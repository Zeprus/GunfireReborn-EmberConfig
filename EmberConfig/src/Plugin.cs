namespace EmberConfig;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using EmberConfig.Core;
using EmberConfig.Public;

/// <summary>
/// BepInEx plugin entry point for EmberConfig.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static ManualLogSource Logger = null!;

    public override void Load()
    {
        Logger = base.Log;
        SettingsRegistry.Current = new SettingsRegistry();
        RegisterEmberConfigSettings();
        AddComponent<SettingsMenuManager>();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} loaded");
    }

    private void RegisterEmberConfigSettings()
    {
        var tabScrollSensitivity = SettingsMenu.Register(Config, new SettingOptions<float>(
            Section: "EmberConfig",
            Key: "TabScrollSensitivity",
            DefaultValue: EmberConfigSettings.DefaultTabScrollSensitivity,
            Description: "How fast the tab list scrolls when the mouse wheel is used.",
            Label: "Tab Scroll Sensitivity",
            Tab: "EmberConfig",
            Group: "EmberConfig",
            AcceptableValues: new AcceptableValueRange<float>(
                EmberConfigSettings.MinTabScrollSensitivity,
                EmberConfigSettings.MaxTabScrollSensitivity),
            OnValueChanged: v => EmberConfigSettings.TabScrollSensitivity = v));

        var tabWidthScaling = SettingsMenu.Register(Config, new SettingOptions<int>(
            Section: "EmberConfig",
            Key: "TabWidthScaling",
            DefaultValue: (int)EmberConfigSettings.DefaultTabWidthScaling,
            Description: "Tab width scaling in percent, based on vanilla tab width.",
            Label: "Tab Width Scaling",
            Tab: "EmberConfig",
            Group: "EmberConfig",
            AcceptableValues: new AcceptableValueRange<int>(
                (int)EmberConfigSettings.MinTabWidthScaling,
                (int)EmberConfigSettings.MaxTabWidthScaling),
            OnValueChanged: v => EmberConfigSettings.TabWidthScaling = v));

        var tabScrollAnimationDuration = SettingsMenu.Register(Config, new SettingOptions<float>(
            Section: "EmberConfig",
            Key: "TabScrollAnimationDuration",
            DefaultValue: EmberConfigSettings.DefaultTabScrollAnimationDuration,
            Description: "Base duration of the tab scroll animation, in seconds. Far tabs may take up to twice as long.",
            Label: "Tab Scroll Animation Duration",
            Tab: "EmberConfig",
            Group: "EmberConfig",
            AcceptableValues: new AcceptableValueRange<float>(
                EmberConfigSettings.MinTabScrollAnimationDuration,
                EmberConfigSettings.MaxTabScrollAnimationDuration),
            OnValueChanged: v => EmberConfigSettings.TabScrollAnimationDuration = v));

        var tabMinFontSize = SettingsMenu.Register(Config, new SettingOptions<float>(
            Section: "EmberConfig",
            Key: "TabMinFontSize",
            DefaultValue: EmberConfigSettings.DefaultTabMinFontSize,
            Description: "Smallest font size tab labels are allowed to shrink to before being truncated.",
            Label: "Tab Min Font Size",
            Tab: "EmberConfig",
            Group: "EmberConfig",
            AcceptableValues: new AcceptableValueRange<float>(
                EmberConfigSettings.MinTabMinFontSize,
                EmberConfigSettings.MaxTabMinFontSize),
            OnValueChanged: v => EmberConfigSettings.TabMinFontSize = v));

        EmberConfigSettings.TabScrollSensitivity = tabScrollSensitivity.Value;
        EmberConfigSettings.TabWidthScaling = tabWidthScaling.Value;
        EmberConfigSettings.TabScrollAnimationDuration = tabScrollAnimationDuration.Value;
        EmberConfigSettings.TabMinFontSize = tabMinFontSize.Value;
    }
}
