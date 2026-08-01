namespace EmberConfig.ExampleMod;

using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using EmberConfig.Public;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

/// <summary>
/// Example BepInEx 6 plugin demonstrating every EmberConfig registration pattern.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(EmberConfigGuid, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    private const string EmberConfigGuid = "zeprus.gunfire.EmberConfig";

    /// <summary>
    /// Shared logger for the example mod. Set during <see cref="Load"/>.
    /// </summary>
    public static ManualLogSource? Logger { get; private set; }

    /// <summary>
    /// Plugin entry point. Checks for EmberConfig, registers the example
    /// MonoBehaviour, and then registers every example setting and keybind.
    /// </summary>
    public override void Load()
    {
        Logger = base.Log;

        if (!IsEmberConfigLoaded())
        {
            Logger?.LogWarning("EmberConfig is not loaded; ExampleMod settings UI will not be available.");
            return;
        }

        ClassInjector.RegisterTypeInIl2Cpp<ExampleMonoBehaviour>();
        var behaviour = AddComponent<ExampleMonoBehaviour>();

        ExampleConfiguration.RegisterAll(Config, behaviour);

        Logger?.LogInfo($"{MyPluginInfo.PLUGIN_NAME} loaded.");
    }

    /// <summary>
    /// Soft-dependency check. Kept in its own non-inlinable method so the type
    /// loader only touches EmberConfig types when we are sure it is present.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static bool IsEmberConfigLoaded()
    {
        return IL2CPPChainloader.Instance?.Plugins.ContainsKey(EmberConfigGuid) ?? false;
    }
}
