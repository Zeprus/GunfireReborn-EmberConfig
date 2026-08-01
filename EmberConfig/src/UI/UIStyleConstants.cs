namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared visual style constants used across EmberConfig UI components.
/// </summary>
public static class UIStyleConstants
{
    /// <summary>
    /// A red ColorBlock used for destructive actions such as resetting visibility.
    /// </summary>
    public static readonly ColorBlock DestructiveColorBlock = new()
    {
        normalColor = new Color(0.25f, 0.05f, 0.05f, 0.42f),
        highlightedColor = new Color(0.40f, 0.12f, 0.12f, 0.50f),
        pressedColor = new Color(0.20f, 0.04f, 0.04f, 0.42f),
        disabledColor = new Color(0.15f, 0.03f, 0.03f, 0.30f),
        colorMultiplier = 1f,
        fadeDuration = 0.1f
    };
}
