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

    private static float tabScrollSensitivity = DefaultTabScrollSensitivity;

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
    /// Fired whenever <see cref="TabScrollSensitivity"/> changes.
    /// </summary>
    public static event Action<float>? TabScrollSensitivityChanged;

    /// <summary>
    /// Applies the current <see cref="TabScrollSensitivity"/> to a <see cref="ScrollRect"/>.
    /// </summary>
    public static void ApplyTo(ScrollRect? scrollRect)
    {
        if (scrollRect is not null)
            scrollRect.scrollSensitivity = TabScrollSensitivity;
    }
}
