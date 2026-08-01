namespace EmberConfig.Patches;

using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Makes clicking the <see cref="TMP_Dropdown"/> itself toggle the list open/closed
/// instead of only opening it. The vanilla <c>OnPointerClick</c> always calls <c>Show</c>;
/// this patch checks the current state and calls <c>Hide</c> when already open.
/// </summary>
[HarmonyPatch(typeof(TMP_Dropdown), "OnPointerClick")]
internal static class TMP_DropdownOnPointerClickPatch
{
    [HarmonyPrefix]
    private static bool Prefix(TMP_Dropdown __instance, PointerEventData eventData)
    {
        _ = eventData;

        if (__instance is null)
            return false;

        if (__instance.IsShow)
            __instance.Hide();
        else
            __instance.Show();

        return false;
    }
}
