namespace SettingsLib.UI;

using System;

/// <summary>
/// Pure math for a circular, center-focused tab carousel.
/// </summary>
internal static class TabCarouselLayout
{
    /// <summary>Number of tabs visible at once (active + 2 previous + 2 next).</summary>
    internal const int VisibleSlotCount = 5;

    /// <summary>Extra slots kept off-screen on each side to make wrap-around seamless.</summary>
    internal const int BufferSlotCount = 2;

    /// <summary>Total slots in the moving belt (visible + buffer).</summary>
    internal const int SlotCount = VisibleSlotCount + BufferSlotCount;

    private const float MaxOffset = (SlotCount - 1) / 2f; // 3

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
    internal static float ShortestDelta(float from, int to, int length)
    {
        if (length <= 0)
            return 0f;

        var delta = Repeat(to - from, length);
        if (delta > length / 2f)
            delta -= length;

        return delta;
    }

    /// <summary>
    /// Returns the content index a slot should show when it sits at the given
    /// <paramref name="offset"/> relative to <paramref name="currentActive"/>.
    /// </summary>
    internal static int GetDesiredContentIndex(float currentActive, int offset)
        => RoundToInt(currentActive + offset);

    /// <summary>
    /// Returns the content index for a slot that just left the left side of the
    /// belt and should re-enter from the right.
    /// </summary>
    internal static int GetRecycledRightContentIndex(float currentActive)
        => CeilingToInt(currentActive + MaxOffset);

    /// <summary>
    /// Returns the content index for a slot that just left the right side of the
    /// belt and should re-enter from the left.
    /// </summary>
    internal static int GetRecycledLeftContentIndex(float currentActive)
        => FloorToInt(currentActive - MaxOffset);

    /// <summary>
    /// Computes the screen-space offset for a slot with the given content index.
    /// </summary>
    internal static float GetVisualPosition(int contentIndex, float currentActive, float step)
        => (contentIndex - currentActive) * step;

    /// <summary>
    /// Whether a slot is far enough outside the belt to be recycled to the
    /// opposite side.
    /// </summary>
    internal static bool IsOffScreen(float visualPosition, float step)
    {
        var halfSpan = SlotCount / 2f * step; // 3.5 slots
        return visualPosition < -halfSpan || visualPosition > halfSpan;
    }

    /// <summary>
    /// Returns alpha and scale for a tab based on its distance from the active tab.
    /// Tabs at the visible edge fade out; off-screen tabs are fully transparent.
    /// </summary>
    internal static (float Alpha, float Scale) GetVisualState(float distance)
    {
        var visibleRadius = VisibleSlotCount / 2f; // 2.5
        var fadeEnd = visibleRadius + 0.5f; // 3.0

        var alpha = 1f - Clamp01((distance - visibleRadius) / (fadeEnd - visibleRadius));
        var scale = 1f - Clamp01(distance / fadeEnd) * 0.15f;

        return (alpha, scale);
    }

    private static float Repeat(float t, float length)
        => length == 0f ? 0f : t - MathF.Floor(t / length) * length;

    private static int RoundToInt(float value)
        => (int)MathF.Round(value, MidpointRounding.AwayFromZero);

    private static int CeilingToInt(float value)
        => (int)MathF.Ceiling(value);

    private static int FloorToInt(float value)
        => (int)MathF.Floor(value);

    private static float Clamp01(float value)
        => value < 0f ? 0f : value > 1f ? 1f : value;
}
