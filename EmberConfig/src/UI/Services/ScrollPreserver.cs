namespace EmberConfig.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Preserves a <see cref="ScrollRect"/>'s vertical scroll position across a short UI action
/// that may rebuild the content underneath it (for example toggling visibility or refreshing rows).
/// </summary>
internal static class ScrollPreserver
{
    public static void Preserve(ScrollRect? scrollRect, string? activeTab, Func<string?> getActiveTab, Action action)
    {
        if (scrollRect is null)
        {
            action();
            return;
        }

        var saved = scrollRect.verticalNormalizedPosition;
        action();

        if (string.Equals(getActiveTab(), activeTab, StringComparison.OrdinalIgnoreCase))
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = saved;
        }
    }

    public static (string? activeTab, float position) Capture(ScrollRect? scrollRect, Func<string?> getActiveTab)
        => (getActiveTab(), scrollRect?.verticalNormalizedPosition ?? 1f);

    public static void Restore(ScrollRect? scrollRect, string? activeTab, Func<string?> getActiveTab, float savedPosition)
    {
        if (scrollRect is null)
            return;

        if (string.Equals(getActiveTab(), activeTab, StringComparison.OrdinalIgnoreCase))
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = savedPosition;
        }
    }
}
