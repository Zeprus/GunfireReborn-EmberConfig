namespace EmberConfig.Patches;

using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reparents the popup list created by <see cref="TMP_Dropdown.Show"/> to the
/// root canvas and removes its dedicated Canvas/GraphicRaycaster so it renders
/// as part of the settings root canvas instead of as a misbehaving nested canvas.
/// </summary>
[HarmonyPatch(typeof(TMP_Dropdown), "Show")]
internal static class TMP_DropdownShowPatch
{
    [HarmonyPostfix]
    private static void Postfix(TMP_Dropdown __instance)
    {
        if (__instance is null)
            return;

        var list = __instance.transform.Find("Dropdown List");
        if (list is null)
            return;

        var canvas = __instance.GetComponentInParent<Canvas>();
        var rootCanvas = canvas?.rootCanvas ?? canvas;
        if (rootCanvas is null)
            return;

        var listRect = list.GetComponent<RectTransform>();
        if (listRect is null)
            return;

        listRect.SetParent(rootCanvas.transform, true);
        listRect.SetAsLastSibling();

        // The dedicated Canvas makes the dropdown list a nested canvas, which
        // does not render correctly in this UI setup. Removing it makes the
        // list a regular child of the settings root canvas and draws it on top.
        if (list.GetComponent<Canvas>() is { } listCanvas)
            Object.Destroy(listCanvas);
        if (list.GetComponent<GraphicRaycaster>() is { } listRaycaster)
            Object.Destroy(listRaycaster);

        // The blocker also gets its own Canvas; without it the clear full-screen
        // image cannot catch outside clicks. Remove the nested canvas so it uses
        // the settings root canvas and keep it behind the list.
        var blocker = rootCanvas.transform.Find("Blocker");
        if (blocker is not null)
        {
            blocker.SetAsFirstSibling();
            if (blocker.GetComponent<Canvas>() is { } blockerCanvas)
                Object.Destroy(blockerCanvas);
            if (blocker.GetComponent<GraphicRaycaster>() is { } blockerRaycaster)
                Object.Destroy(blockerRaycaster);
        }
    }
}
