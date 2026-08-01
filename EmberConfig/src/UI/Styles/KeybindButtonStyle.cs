namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct KeybindItemLayout(
    float Spacing,
    TextAnchor ChildAlignment,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight);

internal readonly record struct KeybindButtonStyle(
    TextAppearance Text,
    TextAppearance NoneText,
    TMP_SpriteAsset? SpriteAsset,
    Color BackgroundColor,
    Sprite? BackgroundSprite,
    Image.Type BackgroundType,
    ColorBlock ButtonColors,
    Selectable.Transition ButtonTransition,
    RectData PrimaryRect,
    RectData SecondaryRect,
    RectData ItemRect,
    KeybindItemLayout ItemLayout)
{
    public uint ClickSoundEventId { get; init; }

    internal static readonly RectData DefaultPrimaryRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(150f, 40f), new Vector2(95f, -25f),
        new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultSecondaryRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(150f, 40f), new Vector2(285f, -25f),
        new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultItemRect = new(
        new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
        new Vector2(380f, 50f), Vector2.zero,
        new Vector2(1f, 0.5f));
}
