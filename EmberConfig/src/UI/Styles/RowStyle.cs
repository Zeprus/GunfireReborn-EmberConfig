namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct RowStyle(
    TextAppearance Title,
    Sprite? BackgroundSprite,
    Color BackgroundColor,
    Color HighlightColor,
    Image.Type BackgroundType,
    float Height,
    float Width,
    float TitleWidth,
    float ItemWidth,
    TextMeshProUGUI? DescriptionText,
    RectData RowRect,
    RectData TitleRect,
    RectData ItemRect)
{
    internal static class Layout
    {
        internal const float DefaultHeight = 50f;
        internal const float DefaultWidth = 1000f;
        internal const float DefaultTitleWidth = 474.8f;
        internal const float DefaultItemWidth = 473.3f;
    }

    internal static readonly RectData DefaultRowRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(Layout.DefaultWidth, Layout.DefaultHeight),
        new Vector2(Layout.DefaultWidth / 2f, -Layout.DefaultHeight / 2f),
        new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultTitleRect = new(
        new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
        new Vector2(Layout.DefaultTitleWidth, Layout.DefaultHeight),
        new Vector2(25f, 0f),
        new Vector2(0f, 0.5f));

    internal static readonly RectData DefaultItemRect = new(
        new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
        new Vector2(Layout.DefaultItemWidth, Layout.DefaultHeight),
        Vector2.zero,
        new Vector2(1f, 0.5f));
}
