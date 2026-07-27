namespace SettingsLib.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// High-level controller for the custom tab bar viewport, sizing, and scrolling.
/// </summary>
internal sealed class TabBarController
{
    private RectTransform? tabSwitchRect;
    private RectTransform? viewportRect;
    private Vector3? originalLocalPosition;

    public void Initialize(Transform tabSwitch, Transform tabContainer)
    {
        if (tabSwitch is null || tabContainer is null)
            return;

        tabSwitchRect = tabSwitch.GetComponent<RectTransform>();
        if (tabSwitchRect is null)
            return;

        var container = tabContainer;
        const string ViewportName = "SL_TabViewport";
        while (container is not null && container.name == ViewportName)
        {
            container = container.parent;
        }

        if (container is null)
            return;

        viewportRect = TabBarViewportFactory.Ensure(tabSwitchRect, container);
        if (viewportRect is null)
            return;

        RefreshSize();
        originalLocalPosition = tabSwitchRect.localPosition;
    }

    public void Reset()
    {
        if (tabSwitchRect is not null && tabSwitchRect && originalLocalPosition.HasValue)
            tabSwitchRect.localPosition = originalLocalPosition.Value;
    }

    public void RefreshSize()
    {
        if (tabSwitchRect is not null && tabSwitchRect)
            TabBarSizeAdjuster.Adjust(tabSwitchRect);
    }

    public void ScrollTo(M1Toggle? activeToggle)
    {
        TabBarScrollController.ScrollTo(activeToggle, tabSwitchRect, viewportRect);
    }
}
