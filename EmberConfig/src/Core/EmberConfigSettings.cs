namespace EmberConfig;

using System;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;

/// <summary>
/// Central settings for EmberConfig itself, exposed through the in-game
/// settings UI and BepInEx configuration.
/// </summary>
public static class EmberConfigSettings
{
    public const float MinTabScrollSensitivity = 20f;
    public const float MaxTabScrollSensitivity = 200f;
    public const float DefaultTabScrollSensitivity = 80f;

    public const float MinTabWidthScaling = 25f;
    public const float MaxTabWidthScaling = 200f;
    public const float DefaultTabWidthScaling = 100f;

    public const float MinTabScrollAnimationDuration = 0.05f;
    public const float MaxTabScrollAnimationDuration = 0.80f;
    public const float DefaultTabScrollAnimationDuration = 0.5f;

    public const float MinTabMinFontSize = 6f;
    public const float MaxTabMinFontSize = 30f;
    public const float DefaultTabMinFontSize = 15f;

    private static float tabScrollSensitivity = DefaultTabScrollSensitivity;
    private static float tabWidthScaling = DefaultTabWidthScaling;
    private static float tabScrollAnimationDuration = DefaultTabScrollAnimationDuration;
    private static float tabMinFontSize = DefaultTabMinFontSize;

    /// <summary>
    /// Gets or sets how fast the tab list scrolls when the mouse wheel is used.
    /// Values are clamped to <see cref="MinTabScrollSensitivity"/> and <see cref="MaxTabScrollSensitivity"/>.
    /// </summary>
    public static float TabScrollSensitivity
    {
        get => tabScrollSensitivity;
        set
        {
            value = Clamp(value, MinTabScrollSensitivity, MaxTabScrollSensitivity);
            if (Abs(value - tabScrollSensitivity) < 0.0001f)
                return;

            tabScrollSensitivity = value;
            TabScrollSensitivityChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Gets or sets the tab width as a percentage of the auto-calculated base width.
    /// Values are clamped to <see cref="MinTabWidthScaling"/> and <see cref="MaxTabWidthScaling"/>.
    /// </summary>
    public static float TabWidthScaling
    {
        get => tabWidthScaling;
        set
        {
            value = Clamp(value, MinTabWidthScaling, MaxTabWidthScaling);
            if (Abs(value - tabWidthScaling) < 0.0001f)
                return;

            tabWidthScaling = value;
            TabWidthScalingChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Gets or sets the base duration of the tab scroll animation, in seconds.
    /// Values are clamped to <see cref="MinTabScrollAnimationDuration"/> and <see cref="MaxTabScrollAnimationDuration"/>.
    /// </summary>
    public static float TabScrollAnimationDuration
    {
        get => tabScrollAnimationDuration;
        set
        {
            value = Clamp(value, MinTabScrollAnimationDuration, MaxTabScrollAnimationDuration);
            if (Abs(value - tabScrollAnimationDuration) < 0.0001f)
                return;

            tabScrollAnimationDuration = value;
        }
    }

    /// <summary>
    /// Gets or sets the smallest font size tab labels are allowed to shrink to
    /// before overflowing with an ellipsis. Configurable so users can decide
    /// how aggressively labels are allowed to compress.
    /// Values are clamped to <see cref="MinTabMinFontSize"/> and <see cref="MaxTabMinFontSize"/>.
    /// </summary>
    public static float TabMinFontSize
    {
        get => tabMinFontSize;
        set
        {
            value = Clamp(value, MinTabMinFontSize, MaxTabMinFontSize);
            if (Abs(value - tabMinFontSize) < 0.0001f)
                return;

            tabMinFontSize = value;
            TabMinFontSizeChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Fired whenever <see cref="TabScrollSensitivity"/> changes.
    /// </summary>
    public static event Action<float>? TabScrollSensitivityChanged;

    /// <summary>
    /// Fired whenever <see cref="TabWidthScaling"/> changes.
    /// </summary>
    public static event Action<float>? TabWidthScalingChanged;

    /// <summary>
    /// Fired whenever <see cref="TabMinFontSize"/> changes.
    /// </summary>
    public static event Action<float>? TabMinFontSizeChanged;

    /// <summary>
    /// Applies the current <see cref="TabScrollSensitivity"/> to a <see cref="ScrollRect"/>.
    /// </summary>
    public static void ApplyTo(ScrollRect? scrollRect)
    {
        if (scrollRect is not null)
            scrollRect.scrollSensitivity = TabScrollSensitivity;
    }
}
