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
    public const float DefaultTabScrollAnimationDuration = 0.25f;

    private static float tabScrollSensitivity = DefaultTabScrollSensitivity;
    private static float tabWidthScaling = DefaultTabWidthScaling;
    private static float tabScrollAnimationDuration = DefaultTabScrollAnimationDuration;

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
            TabScrollAnimationDurationChanged?.Invoke(value);
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
    /// Fired whenever <see cref="TabScrollAnimationDuration"/> changes.
    /// </summary>
    public static event Action<float>? TabScrollAnimationDurationChanged;

    /// <summary>
    /// Applies the current <see cref="TabScrollSensitivity"/> to a <see cref="ScrollRect"/>.
    /// </summary>
    public static void ApplyTo(ScrollRect? scrollRect)
    {
        if (scrollRect is not null)
            scrollRect.scrollSensitivity = TabScrollSensitivity;
    }
}
