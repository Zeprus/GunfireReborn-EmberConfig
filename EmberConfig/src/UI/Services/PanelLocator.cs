namespace EmberConfig.UI;

using System;
using UnityEngine;

/// <summary>
/// Helpers for locating the vanilla settings panel from a child transform.
/// </summary>
internal static class PanelLocator
{
    /// <summary>
    /// Walks up the hierarchy from <paramref name="rowTransform"/> until it
    /// finds the <c>PC_Panel_setting</c> root.
    /// </summary>
    /// <param name="rowTransform">A transform inside the panel.</param>
    /// <returns>The panel root transform, or <c>null</c> if not found.</returns>
    internal static Transform? FindPanelRoot(Transform rowTransform)
    {
        var t = rowTransform;
        while (t is not null)
        {
            if (string.Equals(t.name, "PC_Panel_setting", StringComparison.OrdinalIgnoreCase))
                return t;
            t = t.parent;
        }
        return null;
    }
}
