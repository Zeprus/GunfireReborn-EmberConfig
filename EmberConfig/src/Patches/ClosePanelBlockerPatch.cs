namespace EmberConfig.Patches;

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using EmberConfig.Core;
using UIScript;

[HarmonyPatch]
internal static class ClosePanelBlockerPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        var targets = new List<MethodBase>();

        var pcSettingOnBack = AccessTools.Method(typeof(PCSettingPanel_logic), "OnBack");
        if (pcSettingOnBack is not null)
            targets.Add(pcSettingOnBack);
        else
            Plugin.Logger?.LogWarning($"EmberConfig: could not find {typeof(PCSettingPanel_logic).Name}.OnBack; patch target skipped.");

        var settingPanelOnBack = AccessTools.Method(typeof(SettingPanelManager), "OnBack");
        if (settingPanelOnBack is not null)
            targets.Add(settingPanelOnBack);
        else
            Plugin.Logger?.LogWarning($"EmberConfig: could not find {typeof(SettingPanelManager).Name}.OnBack; patch target skipped.");

        var keyboardPanelOnBack = AccessTools.Method(typeof(KeyBoardPanelManager), "OnBack");
        if (keyboardPanelOnBack is not null)
            targets.Add(keyboardPanelOnBack);
        else
            Plugin.Logger?.LogWarning($"EmberConfig: could not find {typeof(KeyBoardPanelManager).Name}.OnBack; patch target skipped.");

        var controllerKeyOnBack = AccessTools.Method(typeof(PCControllerKey_Logic), "OnBack");
        if (controllerKeyOnBack is not null)
            targets.Add(controllerKeyOnBack);
        else
            Plugin.Logger?.LogWarning($"EmberConfig: could not find {typeof(PCControllerKey_Logic).Name}.OnBack; patch target skipped.");

        if (targets.Count == 0)
            Plugin.Logger?.LogWarning("EmberConfig: ClosePanelBlockerPatch has no target methods; patch will be empty.");

        foreach (var target in targets)
            yield return target;
    }

    private static bool Prefix()
    {
        if (SettingsPanelState.IsCapturing || SettingsPanelState.IsBlockingClose)
            return false;

        return true;
    }
}
