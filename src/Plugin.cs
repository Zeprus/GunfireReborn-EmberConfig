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
        var entry = SettingsMenu.Register(Config, new SettingOptions<float>(
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

        EmberConfigSettings.TabScrollSensitivity = entry.Value;
    }
}
