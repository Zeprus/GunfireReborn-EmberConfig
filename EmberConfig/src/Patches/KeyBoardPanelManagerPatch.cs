namespace EmberConfig.Patches;

using HarmonyLib;
using EmberConfig.Core;
using UIScript;

[HarmonyPatch(typeof(KeyBoardPanelManager), "ShowShowItem", new[] { typeof(int) })]
internal static class KeyBoardPanelManagerPatch
{
    [HarmonyPostfix]
    internal static void Postfix(int ID)
    {
        SettingsPanelState.RefreshKeybindPanel(ID);
    }
}
