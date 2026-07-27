namespace SettingsLib.UI;

using System;

/// <summary>
/// Pure math for keeping a tab bar's active tab inside a viewport while
/// preventing the tab bar from scrolling past its first or last tab.
/// </summary>
internal static class TabBarLayout
{
    /// <summary>
    /// Computes the horizontal offset to apply to a tab bar so the active tab
    /// is fully visible and, when possible, centered inside the viewport.
    /// </summary>
    /// <param name="viewportMin">Left edge of the visible viewport.</param>
    /// <param name="viewportMax">Right edge of the visible viewport.</param>
    /// <param name="contentMin">Left edge of the tab bar content.</param>
    /// <param name="contentMax">Right edge of the tab bar content.</param>
    /// <param name="activeMin">Left edge of the active tab.</param>
    /// <param name="activeMax">Right edge of the active tab.</param>
    /// <param name="currentOffset">Current horizontal offset of the tab bar.</param>
    /// <param name="recenterIfVisible">
    /// When <c>true</c>, re-centers the active tab even if it is already fully
    /// visible. When <c>false</c> (default), no offset change is returned while
    /// the active tab is already fully visible.
    /// </param>
    /// <returns>The desired horizontal offset for the tab bar.</returns>
    internal static float ComputeScrollOffset(
        float viewportMin,
        float viewportMax,
        float contentMin,
        float contentMax,
        float activeMin,
        float activeMax,
        float currentOffset,
        bool recenterIfVisible = false)
    {
        var viewportCenter = (viewportMin + viewportMax) * 0.5f;
        var activeCenter = (activeMin + activeMax) * 0.5f;

        var lower = Math.Min(viewportMax - contentMax, viewportMin - contentMin);
        var upper = Math.Max(viewportMax - contentMax, viewportMin - contentMin);

        if (!recenterIfVisible)
        {
            var activeLeft = currentOffset + activeMin;
            var activeRight = currentOffset + activeMax;
            if (activeLeft >= viewportMin && activeRight <= viewportMax)
            {
                return currentOffset;
            }
        }

        var target = viewportCenter - activeCenter;
        return Math.Clamp(target, lower, upper);
    }
}
