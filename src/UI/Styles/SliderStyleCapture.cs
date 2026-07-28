namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class SliderStyleCapture
{
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
            : SliderStyle.DefaultBackgroundRect;
        var bgRect = bg is not null
            ? RectData.From(bg.GetComponent<RectTransform>())
            : SliderStyle.DefaultBgRect;
        var fillAreaRect = fillArea is not null
            ? RectData.From(fillArea.GetComponent<RectTransform>())
            : SliderStyle.DefaultFillAreaRect;
        var fillRectData = fill is not null
            ? RectData.From(fill.GetComponent<RectTransform>())
            : SliderStyle.DefaultFillRect;
        var handleSlideAreaRect = handleArea is not null
            ? RectData.From(handleArea.GetComponent<RectTransform>())
            : SliderStyle.DefaultHandleSlideAreaRect;
        var handleRectData = handle is not null
            ? RectData.From(handle.GetComponent<RectTransform>())
            : SliderStyle.DefaultHandleRect;
        var numRect = num is not null
            ? RectData.From(num.GetComponent<RectTransform>())
            : SliderStyle.DefaultNumRect;

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
