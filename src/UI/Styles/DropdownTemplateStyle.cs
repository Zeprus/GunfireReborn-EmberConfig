namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual style for the dropdown list template: background, viewport, content,
/// each list item, highlight, background, checkmark and label.
/// </summary>
internal readonly record struct DropdownTemplateStyle(
    RectData TemplateRect,
    Sprite? TemplateSprite,
    Color TemplateBgColor,
    Image.Type TemplateImageType,
    RectData ViewportRect,
    RectData ContentRect,
    RectData TemplateItemRect,
    RectData TemplateHighlightRect,
    Sprite? TemplateHighlightSprite,
    Color TemplateHighlightColor,
    Image.Type TemplateHighlightType,
    RectData ItemBackgroundRect,
    Sprite? ItemBgSprite,
    Color ItemBgColor,
    Image.Type ItemBgType,
    RectData ItemCheckmarkRect,
    Sprite? ItemCheckmarkSprite,
    Color ItemCheckmarkColor,
    Image.Type ItemCheckmarkType,
    RectData ItemLabelRect,
    TextAppearance ItemLabelTextAppearance,
    TextAlignmentOptions ItemLabelAlignment)
{
    internal static DropdownTemplateStyle Default(Sprite? fallbackSprite, TextAppearance fallbackText) =>
        new(
            new RectData(Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 150f), Vector2.zero, new Vector2(0.5f, 1f)),
            fallbackSprite,
            new Color(0f, 0f, 0f, 0.8f),
            Image.Type.Simple,
            new RectData(Vector2.zero, Vector2.one, new Vector2(-3f, 0f), Vector2.zero, new Vector2(0f, 1f)),
            new RectData(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 28f), Vector2.zero, new Vector2(0.5f, 1f)),
            new RectData(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 28f), Vector2.zero, new Vector2(0.5f, 0.5f)),
            new RectData(Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f)),
            fallbackSprite,
            new Color(0.877f, 0.277f, 0.277f, 1f),
            Image.Type.Sliced,
            new RectData(Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f)),
            fallbackSprite,
            new Color(0.749f, 0.675f, 0.471f, 0.502f),
            Image.Type.Simple,
            new RectData(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(274.5f, 28f), new Vector2(137.3f, 0f), new Vector2(0.5f, 0.5f)),
            fallbackSprite,
            new Color(0.749f, 0.675f, 0.471f, 0f),
            Image.Type.Simple,
            new RectData(Vector2.zero, Vector2.one, new Vector2(-30f, -3f), new Vector2(15f, 0f), new Vector2(0.5f, 0.5f)),
            fallbackText,
            TextAlignmentOptions.Left);
}
