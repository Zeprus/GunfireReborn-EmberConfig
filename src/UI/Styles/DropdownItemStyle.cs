namespace SettingsLib.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual style for the collapsed dropdown row: the item background, label and arrow.
/// </summary>
internal readonly record struct DropdownItemStyle(
    Sprite? ItemSprite,
    Color ItemColor,
    Image.Type ItemType,
    RectData ItemRect,
    RectData LabelRect,
    TextAppearance LabelTextAppearance,
    TextAlignmentOptions LabelAlignment,
    RectData ArrowRect,
    Sprite? ArrowSprite,
    Color ArrowColor,
    Image.Type ArrowType)
{
    internal static DropdownItemStyle Default(Sprite? fallbackSprite, TextAppearance fallbackText) =>
        new(
            fallbackSprite,
            Color.black,
            Image.Type.Sliced,
            new RectData(Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero),
            new RectData(Vector2.zero, Vector2.one, new Vector2(-40f, 0f), new Vector2(20f, 0f), new Vector2(0.5f, 0.5f)),
            fallbackText,
            TextAlignmentOptions.Center,
            new RectData(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(20f, 20f), new Vector2(-10f, 0f), new Vector2(0.5f, 0.5f)),
            fallbackSprite,
            Color.white,
            Image.Type.Simple);
}
