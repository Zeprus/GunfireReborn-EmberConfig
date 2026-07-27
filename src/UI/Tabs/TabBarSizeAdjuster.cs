namespace SettingsLib.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grows the tab bar's width to match its preferred content width so child
/// positions are deterministic for scrolling calculations.
/// </summary>
internal static class TabBarSizeAdjuster
{
    internal static void Adjust(RectTransform tabSwitchRect)
    {
        if (tabSwitchRect is null || !tabSwitchRect)
            return;

        var layoutGroup = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup is not null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(tabSwitchRect);

        var preferred = layoutGroup?.preferredWidth ?? 0f;
        if (preferred > 0f)
        {
            var size = tabSwitchRect.sizeDelta;
            size.x = Mathf.Max(preferred, tabSwitchRect.sizeDelta.x);
            tabSwitchRect.sizeDelta = size;
        }
    }
}
