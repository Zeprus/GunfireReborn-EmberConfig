namespace SettingsLib;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using SettingsLib.Core;

/// <summary>
/// BepInEx plugin entry point for SettingsLib.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static ManualLogSource Logger = null!;

    public override void Load()
    {
        Logger = base.Log;
        SettingsRegistry.Current = new SettingsRegistry();
        AddComponent<SettingsMenuManager>();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} loaded");
    }
}
