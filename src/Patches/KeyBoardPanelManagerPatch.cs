namespace SettingsLib.Patches;

using HarmonyLib;
using SettingsLib.Core;
using UIScript;

[HarmonyPatch(typeof(KeyBoardPanelManager), "ShowShowItem", new[] { typeof(int) })]
public static class KeyBoardPanelManagerPatch
{
    [HarmonyPostfix]
    public static void Postfix(int ID)
    {
        SettingsPanelState.RefreshKeybindPanel(ID);
    }
}
