namespace EmberConfig.UI;

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
        if (appearance.Font is not null)
        {
            text.font = appearance.Font;
            if (appearance.FontMaterial == appearance.Font.material)
                text.fontSharedMaterial = appearance.FontMaterial;
        }
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

    /// <summary>
    /// Applies a <see cref="TextAppearance"/> and sets the text content, disables
    /// raycast targeting, and optionally overrides alignment.
    /// </summary>
    internal static void Apply(TextMeshProUGUI text, TextAppearance appearance, string content, TextAlignmentOptions? alignment = null)
    {
        Apply(text, appearance);
        text.text = content;
        text.raycastTarget = false;

        if (alignment.HasValue)
            text.alignment = alignment.Value;
    }
}
