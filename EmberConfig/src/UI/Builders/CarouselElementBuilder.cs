namespace EmberConfig.UI;

using DYControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class CarouselElementBuilder
{
    internal static Transform Build(string name, RowStyle style, CarouselStyle carouselStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var item = RowElementBuilder.CreateItem(style, root);
        carouselStyle.ItemRect.Apply(item.GetComponent<RectTransform>());

        var mutiClickGroupObj = RowElementBuilder.CreateObject("MutiClickGroup", item.transform);
        carouselStyle.MutiClickGroupRect.Apply(mutiClickGroupObj.GetComponent<RectTransform>());

        var previousButton = BuildArrowButton("previous", carouselStyle.PreviousButtonRect, carouselStyle, carouselStyle.ArrowImageRect, mutiClickGroupObj.transform, flipHorizontal: true);
        var nextButton = BuildArrowButton("next", carouselStyle.NextButtonRect, carouselStyle, carouselStyle.NextArrowImageRect, mutiClickGroupObj.transform, flipHorizontal: false);

        var settingInfoObj = RowElementBuilder.CreateObject("setting_info", mutiClickGroupObj.transform);
        carouselStyle.SettingInfoRect.Apply(settingInfoObj.GetComponent<RectTransform>());

        var nowsetionObj = RowElementBuilder.CreateObject("nowsetion", settingInfoObj.transform);
        carouselStyle.NowsetionRect.Apply(nowsetionObj.GetComponent<RectTransform>());

        var textObj = RowElementBuilder.CreateObject("Text", nowsetionObj.transform);
        RowElementBuilder.SetRect(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var valueText = RowElementBuilder.AddText(textObj, carouselStyle.ValueText, string.Empty);
        valueText.raycastTarget = false;

        var dotGroupObj = RowElementBuilder.CreateObject("Toggle_group", settingInfoObj.transform);
        carouselStyle.DotGroupRect.Apply(dotGroupObj.GetComponent<RectTransform>());

        var dotHlg = dotGroupObj.AddComponent<HorizontalLayoutGroup>();
        dotHlg.spacing = carouselStyle.DotGroupLayout.Spacing;
        dotHlg.padding = new RectOffset(
            carouselStyle.DotGroupLayout.PaddingLeft,
            carouselStyle.DotGroupLayout.PaddingRight,
            carouselStyle.DotGroupLayout.PaddingTop,
            carouselStyle.DotGroupLayout.PaddingBottom);
        dotHlg.childAlignment = carouselStyle.DotGroupLayout.ChildAlignment;
        dotHlg.childControlWidth = carouselStyle.DotGroupLayout.ChildControlWidth;
        dotHlg.childControlHeight = carouselStyle.DotGroupLayout.ChildControlHeight;
        dotHlg.childForceExpandWidth = carouselStyle.DotGroupLayout.ChildForceExpandWidth;
        dotHlg.childForceExpandHeight = carouselStyle.DotGroupLayout.ChildForceExpandHeight;

        VanillaComponentApplier.ApplyToRow(root.transform, previousButton);

        return root.transform;
    }

    private static M1Button BuildArrowButton(string name, RectData rect, CarouselStyle style, RectData imageRect, Transform parent, bool flipHorizontal)
    {
        var buttonObj = RowElementBuilder.CreateObject(name, parent);
        rect.Apply(buttonObj.GetComponent<RectTransform>());

        var imageObj = RowElementBuilder.CreateObject("img", buttonObj.transform);
        imageRect.Apply(imageObj.GetComponent<RectTransform>());

        var image = RowElementBuilder.AddImage(imageObj, style.ArrowImageSprite, style.ArrowImageType, style.ArrowImageColor);
        image.raycastTarget = true;

        var imageTransform = image.GetComponent<RectTransform>();
        imageTransform.localEulerAngles = new Vector3(0f, 0f, flipHorizontal ? -90f : 90f);

        var button = buttonObj.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = style.ArrowButtonColorBlock;
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(buttonObj.transform, addDySelect: true, addAudio: true);

        var dySelect = buttonObj.GetComponent<DYSelect>();
        if (dySelect is not null)
            dySelect.isCurBtn = true;

        return button;
    }

    internal static GameObject CreateDot(CarouselStyle style, int index, Transform parent)
    {
        var dotObj = RowElementBuilder.CreateObject(index.ToString(), parent);
        style.DotRootRect.Apply(dotObj.GetComponent<RectTransform>());

        var dotImageObj = RowElementBuilder.CreateObject("Background", dotObj.transform);
        style.DotChildRect.Apply(dotImageObj.GetComponent<RectTransform>());

        var dotImage = RowElementBuilder.AddImage(dotImageObj, style.DotSprite, style.DotType, style.DotBackgroundColor);
        dotImage.raycastTarget = true;

        return dotObj;
    }
}
