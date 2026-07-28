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
        var pcSettingOnBack = AccessTools.Method(typeof(PCSettingPanel_logic), "OnBack");
        if (pcSettingOnBack is not null)
            yield return pcSettingOnBack;

        var settingPanelOnBack = AccessTools.Method(typeof(SettingPanelManager), "OnBack");
        if (settingPanelOnBack is not null)
            yield return settingPanelOnBack;

        var keyboardPanelOnBack = AccessTools.Method(typeof(KeyBoardPanelManager), "OnBack");
        if (keyboardPanelOnBack is not null)
            yield return keyboardPanelOnBack;

        var controllerKeyOnBack = AccessTools.Method(typeof(PCControllerKey_Logic), "OnBack");
        if (controllerKeyOnBack is not null)
            yield return controllerKeyOnBack;
    }

    private static bool Prefix()
    {
        if (SettingsPanelState.IsCapturing || SettingsPanelState.IsBlockingClose)
            return false;

        return true;
    }
}
