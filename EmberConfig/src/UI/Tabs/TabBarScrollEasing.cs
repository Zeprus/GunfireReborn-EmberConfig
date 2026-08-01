namespace EmberConfig.UI;

/// <summary>
/// Pure math helpers for the tab bar scroll animation. Kept free of UnityEngine
/// so it can be unit-tested without IL2CPP initialisation issues.
/// </summary>
internal static class TabBarScrollEasing
{
    /// <summary>
    /// Returns an ease-out cubic value for <paramref name="t"/> in [0, 1].
    /// </summary>
    internal static float EaseOutCubic(float t)
    {
        var oneMinus = 1f - t;
        return 1f - oneMinus * oneMinus * oneMinus;
    }

    /// <summary>
    /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    internal static float Lerp(float a, float b, float t) => a + (b - a) * t;

    /// <summary>
    /// Computes the transition duration from a base duration and the normalized
    /// scroll distance. Far tabs take up to twice the base duration.
    /// </summary>
    internal static float ComputeDuration(float baseDuration, float normalizedDistance)
    {
        if (baseDuration <= 0f)
            return 0.05f;

        var min = 0.05f;
        var max = baseDuration * 2f;
        var raw = baseDuration * (1f + normalizedDistance);
        return raw < min ? min : (raw > max ? max : raw);
    }
}
