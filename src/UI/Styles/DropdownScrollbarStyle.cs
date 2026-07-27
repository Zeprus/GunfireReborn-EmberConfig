namespace SettingsLib.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual style for the dropdown list scrollbar.
/// </summary>
internal readonly record struct DropdownScrollbarStyle(
    RectData ScrollbarRect,
    Sprite? ScrollbarSprite,
    Color ScrollbarColor,
    Image.Type ScrollbarType,
    RectData SlidingAreaRect,
    RectData HandleRect,
    Sprite? HandleSprite,
    Color HandleColor,
    Image.Type HandleType)
{
    internal static DropdownScrollbarStyle Default(Sprite? fallbackSprite) =>
        new(
            new RectData(new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(6f, 0f), Vector2.zero, new Vector2(1f, 1f)),
            fallbackSprite,
            new Color(0.584f, 0.518f, 0.341f, 0.2f),
            Image.Type.Sliced,
            new RectData(Vector2.zero, Vector2.one, new Vector2(0f, -10f), new Vector2(0f, 5f), new Vector2(0.5f, 0.5f)),
            new RectData(Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f)),
            fallbackSprite,
            new Color(0.584f, 0.518f, 0.341f, 0.8f),
            Image.Type.Sliced);
}
