namespace SettingsLib.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct SliderStyle(
    Color BackgroundColor,
    Color BgColor,
    Color FillColor,
    Color HandleColor,
    Sprite? BackgroundSprite,
    Sprite? FillSprite,
    Sprite? HandleSprite,
    Image.Type FillImageType,
    Image.FillMethod FillFillMethod,
    RectData SliderPcUnitRect,
    RectData SliderRect,
    RectData BackgroundRect,
    RectData BgRect,
    RectData FillAreaRect,
    RectData FillRect,
    RectData HandleSlideAreaRect,
    RectData HandleRect,
    RectData NumRect,
    TextAppearance NumTextAppearance)
{
    public uint ClickSoundEventId { get; init; }

    internal static readonly RectData DefaultBackgroundRect = new(
        new Vector2(0f, 0.3f), new Vector2(0f, 0.3f),
        new Vector2(291f, 50f), new Vector2(236.7f, 12.5f), new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultBgRect = new(
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(291f, 8f), Vector2.zero, new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultFillAreaRect = new(
        new Vector2(0f, 0.3f), new Vector2(1f, 0.8f),
        new Vector2(-6f, -5.7f), new Vector2(54.7f, 0.1f), new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultFillRect = new(
        Vector2.zero, Vector2.one,
        new Vector2(-67f, -11f), Vector2.zero, new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultHandleSlideAreaRect = new(
        Vector2.zero, Vector2.one,
        new Vector2(-76.3f, 0f), new Vector2(54.4f, 0f), new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultHandleRect = new(
        new Vector2(0.3f, 0f), new Vector2(0.3f, 1f),
        new Vector2(6f, -28f), new Vector2(0.1f, 0f), new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultNumRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(40f, 30f), new Vector2(413f, -40f), new Vector2(0f, 0f));
}
