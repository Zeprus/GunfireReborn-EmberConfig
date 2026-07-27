namespace SettingsLib.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class SliderElementBuilder
{
    internal static Transform Build(string name, RowStyle style, SliderStyle sliderStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var item = RowElementBuilder.CreateItem(style, root);

        var sliderPcUnit = RowElementBuilder.CreateObject("Slider_PCunit", item.transform);
        sliderStyle.SliderPcUnitRect.Apply(sliderPcUnit.GetComponent<RectTransform>());
        var hlg = sliderPcUnit.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 0f;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var sliderObj = RowElementBuilder.CreateObject("Slider", sliderPcUnit.transform);
        sliderStyle.SliderRect.Apply(sliderObj.GetComponent<RectTransform>());

        var slider = sliderObj.AddComponent<M1Slider>();
        sliderObj.AddComponent<CanvasRenderer>();

        var background = RowElementBuilder.CreateObject("Background", sliderObj.transform);
        sliderStyle.BackgroundRect.Apply(background.GetComponent<RectTransform>());
        RowElementBuilder.AddImage(background, sliderStyle.BackgroundSprite, Image.Type.Simple, sliderStyle.BackgroundColor);

        var bg = RowElementBuilder.CreateObject("bg", background.transform);
        sliderStyle.BgRect.Apply(bg.GetComponent<RectTransform>());
        var bgImage = RowElementBuilder.AddImage(bg, sliderStyle.BackgroundSprite, Image.Type.Simple, sliderStyle.BgColor);
        bgImage.raycastTarget = false;

        var fillArea = RowElementBuilder.CreateObject("Fill Area", sliderObj.transform);
        sliderStyle.FillAreaRect.Apply(fillArea.GetComponent<RectTransform>());

        var fill = RowElementBuilder.CreateObject("Fill", fillArea.transform);
        sliderStyle.FillRect.Apply(fill.GetComponent<RectTransform>());
        var fillImage = RowElementBuilder.AddImage(fill, sliderStyle.FillSprite, sliderStyle.FillImageType, sliderStyle.FillColor);
        fillImage.fillMethod = sliderStyle.FillFillMethod;
        fillImage.raycastTarget = false;

        var handleArea = RowElementBuilder.CreateObject("Handle Slide Area", sliderObj.transform);
        sliderStyle.HandleSlideAreaRect.Apply(handleArea.GetComponent<RectTransform>());

        var handle = RowElementBuilder.CreateObject("Handle", handleArea.transform);
        sliderStyle.HandleRect.Apply(handle.GetComponent<RectTransform>());
        var handleImage = RowElementBuilder.AddImage(handle, sliderStyle.HandleSprite, Image.Type.Simple, sliderStyle.HandleColor);
        handleImage.raycastTarget = false;

        slider.targetGraphic = handleImage;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.interactable = true;

        var numObj = RowElementBuilder.CreateObject("Num", sliderPcUnit.transform);
        sliderStyle.NumRect.Apply(numObj.GetComponent<RectTransform>());
        RowElementBuilder.AddText(numObj, sliderStyle.NumTextAppearance, string.Empty, TextAlignmentOptions.Right);

        VanillaComponentApplier.ApplyToRow(root.transform, slider);
        VanillaComponentApplier.AttachAudio(slider.transform);

        return root.transform;
    }
}
