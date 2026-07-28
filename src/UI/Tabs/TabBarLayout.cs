namespace SettingsLib.UI;

using System;

/// <summary>
/// Pure math for a horizontally scrollable, left-anchored tab bar.
/// </summary>
internal static class TabBarLayout
{
    /// <summary>
    /// Circular modulo for tab indices. Returns a value in <c>[0, length)</c>.
    /// </summary>
    internal static int Mod(int value, int length)
    {
        if (length <= 0)
            return 0;

        var remainder = value % length;
        return remainder < 0 ? remainder + length : remainder;
    }

    /// <summary>
    /// Returns the wrapped offset from <paramref name="from"/> to <paramref name="to"/>
    /// choosing the shortest circular path. Result is in <c>[-length/2, length/2]</c>.
    /// </summary>
    internal static int ShortestDelta(int from, int to, int length)
    {
        if (length <= 0)
            return 0;

        var delta = Mod(to - from, length);
        if (delta > length / 2f)
            delta -= length;

        return delta;
    }

    /// <summary>
    /// Computes the local content offset that keeps the active tab's left edge
    /// aligned with the viewport's left edge, clamped so the content cannot
    /// scroll past its bounds.
    /// </summary>
    internal static float ComputeScrollOffset(float viewportWidth, float contentWidth, float activeLeft)
    {
        var scrollable = contentWidth - viewportWidth;
        if (scrollable <= 0f)
            return 0f;

        var desired = -activeLeft;
        if (desired < -scrollable)
            return -scrollable;
        if (desired > 0f)
            return 0f;

        return desired;
    }

    /// <summary>
    /// Computes the <see cref="UnityEngine.UI.ScrollRect.horizontalNormalizedPosition"/>
    /// value that keeps the active tab's left edge aligned with the viewport's
    /// left edge. Returns a value in <c>[0, 1]</c>.
    /// </summary>
    internal static float ComputeHorizontalNormalizedPosition(float viewportWidth, float contentWidth, float activeLeft)
    {
        var scrollable = contentWidth - viewportWidth;
        if (scrollable <= 0f)
            return 0f;

        var offset = ComputeScrollOffset(viewportWidth, contentWidth, activeLeft);
        return -offset / scrollable;
    }
}
