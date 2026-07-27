namespace SettingsLib.UI;

using TMPro;

/// <summary>
/// Shared helper for applying a <see cref="TextAppearance"/> to a TextMeshPro text.
/// </summary>
internal static class TextAppearanceApplier
{
    /// <summary>
    /// Applies font, color, size, alignment, and other text appearance settings.
    /// </summary>
    /// <param name="text">The text component to style.</param>
    /// <param name="appearance">The appearance to apply.</param>
    internal static void Apply(TextMeshProUGUI text, TextAppearance appearance)
    {
        text.font = appearance.Font;
        text.fontSharedMaterial = appearance.FontMaterial;
        text.fontSize = appearance.FontSize;
        text.fontSizeMin = appearance.FontSizeMin;
        text.fontSizeMax = appearance.FontSizeMax;
        text.color = appearance.Color;
        text.alignment = appearance.Alignment;
        text.fontStyle = appearance.FontStyle;
        text.outlineWidth = appearance.OutlineWidth;
        text.enableWordWrapping = appearance.EnableWordWrapping;
        text.enableAutoSizing = appearance.EnableAutoSizing;
        text.overflowMode = appearance.OverflowMode;
    }
}
