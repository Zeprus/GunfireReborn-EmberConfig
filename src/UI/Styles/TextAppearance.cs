namespace EmberConfig.UI;

using TMPro;
using UnityEngine;

internal readonly record struct TextAppearance(
    TMP_FontAsset Font,
    Material FontMaterial,
    float FontSize,
    Color Color,
    TextAlignmentOptions Alignment = TextAlignmentOptions.Center,
    FontStyles FontStyle = FontStyles.Normal,
    float OutlineWidth = 0f,
    bool EnableWordWrapping = false,
    bool EnableAutoSizing = false,
    TextOverflowModes OverflowMode = TextOverflowModes.Overflow,
    float FontSizeMin = 0f,
    float FontSizeMax = 0f)
{
    internal static TextAppearance From(TextMeshProUGUI tmp, float fallbackFontSize = 20f) => new(
        tmp.font,
        tmp.fontSharedMaterial,
        tmp.fontSize > 0 ? tmp.fontSize : fallbackFontSize,
        tmp.color,
        tmp.alignment,
        tmp.fontStyle,
        tmp.outlineWidth,
        tmp.enableWordWrapping,
        tmp.enableAutoSizing,
        tmp.overflowMode,
        tmp.fontSizeMin,
        tmp.fontSizeMax);
}
