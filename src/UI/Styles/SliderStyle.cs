namespace SettingsLib.UI;

using System;
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

    internal static SliderStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        var sliderPcUnit = FindSliderPcUnit(panelRoot);
        if (sliderPcUnit is null)
            return null;

        var sliderPcUnitRect = RectData.From(sliderPcUnit.GetComponent<RectTransform>());

        var slider = FindChild(sliderPcUnit, "Slider");
        if (slider is null)
            return null;
        var sliderRect = RectData.From(slider.GetComponent<RectTransform>());

        var background = FindChild(slider, "Background");
        var backgroundImage = background?.GetComponent<Image>();
        var bg = background is not null ? FindChild(background, "bg") : null;
        var bgImage = bg?.GetComponent<Image>();

        var fillArea = FindChild(slider, "Fill Area");
        var fill = fillArea is not null ? FindChild(fillArea, "Fill") : null;
        var fillImage = fill?.GetComponent<Image>();

        var handleArea = FindChild(slider, "Handle Slide Area");
        var handle = handleArea is not null ? FindChild(handleArea, "Handle") : null;
        var handleImage = handle?.GetComponent<Image>();

        var num = FindChild(sliderPcUnit, "Num");
        var numTmp = num?.GetComponent<TextMeshProUGUI>();

        var bgSprite = bgImage?.sprite ?? backgroundImage?.sprite ?? fallbackSprite;
        var fillSprite = fillImage?.sprite ?? fallbackSprite;
        var handleSprite = handleImage?.sprite ?? fallbackSprite;

        var backgroundColor = backgroundImage?.color ?? new Color(0.329f, 0.302f, 0.302f, 0f);
        var bgColor = bgImage?.color ?? new Color(0.329f, 0.302f, 0.302f, 1f);
        var fillColor = fillImage?.color ?? Color.white;
        var handleColor = handleImage?.color ?? Color.white;

        var fillImageType = fillImage?.type ?? Image.Type.Filled;
        var fillFillMethod = fillImage?.fillMethod ?? Image.FillMethod.Horizontal;

        var backgroundRect = background is not null
            ? RectData.From(background.GetComponent<RectTransform>())
            : DefaultBackgroundRect;
        var bgRect = bg is not null
            ? RectData.From(bg.GetComponent<RectTransform>())
            : DefaultBgRect;
        var fillAreaRect = fillArea is not null
            ? RectData.From(fillArea.GetComponent<RectTransform>())
            : DefaultFillAreaRect;
        var fillRectData = fill is not null
            ? RectData.From(fill.GetComponent<RectTransform>())
            : DefaultFillRect;
        var handleSlideAreaRect = handleArea is not null
            ? RectData.From(handleArea.GetComponent<RectTransform>())
            : DefaultHandleSlideAreaRect;
        var handleRectData = handle is not null
            ? RectData.From(handle.GetComponent<RectTransform>())
            : DefaultHandleRect;
        var numRect = num is not null
            ? RectData.From(num.GetComponent<RectTransform>())
            : DefaultNumRect;

        var numTextAppearance = numTmp is not null
            ? TextAppearance.From(numTmp)
            : fallbackText with { Color = new Color(0.584f, 0.518f, 0.341f, 1f) };

        return new SliderStyle(
            backgroundColor,
            bgColor,
            fillColor,
            handleColor,
            bgSprite,
            fillSprite,
            handleSprite,
            fillImageType,
            fillFillMethod,
            sliderPcUnitRect,
            sliderRect,
            backgroundRect,
            bgRect,
            fillAreaRect,
            fillRectData,
            handleSlideAreaRect,
            handleRectData,
            numRect,
            numTextAppearance)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(slider),
        };
    }

    private static readonly RectData DefaultBackgroundRect = new(
        new Vector2(0f, 0.3f), new Vector2(0f, 0.3f),
        new Vector2(291f, 50f), new Vector2(236.7f, 12.5f), new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultBgRect = new(
        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
        new Vector2(291f, 8f), Vector2.zero, new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultFillAreaRect = new(
        new Vector2(0f, 0.3f), new Vector2(1f, 0.8f),
        new Vector2(-6f, -5.7f), new Vector2(54.7f, 0.1f), new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultFillRect = new(
        Vector2.zero, Vector2.one,
        new Vector2(-67f, -11f), Vector2.zero, new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultHandleSlideAreaRect = new(
        Vector2.zero, Vector2.one,
        new Vector2(-76.3f, 0f), new Vector2(54.4f, 0f), new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultHandleRect = new(
        new Vector2(0.3f, 0f), new Vector2(0.3f, 1f),
        new Vector2(6f, -28f), new Vector2(0.1f, 0f), new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultNumRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(40f, 30f), new Vector2(413f, -40f), new Vector2(0f, 0f));

    private static Transform? FindSliderPcUnit(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (string.Equals(transforms[i].name, "Slider_PCunit", StringComparison.OrdinalIgnoreCase))
                return transforms[i];
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (t.GetComponent<Slider>() is not null)
                return t.parent;
        }

        return null;
    }

    private static Transform? FindChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                return child;
        }
        return null;
    }
}
