namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct GroupHeaderStyle(
    TextAppearance Header,
    float Spacing,
    float SubGroupHeaderHeight,
    RectOffset TitlePadding,
    float TitleSpacing,
    RectData HeaderTextRect,
    RectData DividerRect,
    Color DividerColor,
    Sprite? DividerSprite,
    Image.Type DividerType)
{
    internal static readonly RectData DefaultHeaderTextRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(0f, 33f), new Vector2(0f, -16.5f),
        new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultDividerRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(0f, 4f), new Vector2(0f, -2f),
        new Vector2(0.5f, 0.5f));

    internal static GroupHeaderStyle Default(TextAppearance header) =>
        new(
            header with { FontSize = 24f },
            10f,
            30f,
            new RectOffset(20, 20, 10, 10),
            0f,
            DefaultHeaderTextRect,
            DefaultDividerRect,
            new Color(0.584f, 0.518f, 0.341f, 0.3f),
            null,
            Image.Type.Sliced);
}
