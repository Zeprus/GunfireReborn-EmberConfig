namespace EmberConfig.UI;

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

        var mutiClickGroupObj = RowElementBuilder.CreateObject("MutiClickGroup", item.transform);
        carouselStyle.MutiClickGroupRect.Apply(mutiClickGroupObj.GetComponent<RectTransform>());

        var previousButton = BuildArrowButton("previous", carouselStyle.PreviousButtonRect, carouselStyle, mutiClickGroupObj.transform, false);
        var nextButton = BuildArrowButton("next", carouselStyle.NextButtonRect, carouselStyle, mutiClickGroupObj.transform, true);

        var settingInfoObj = RowElementBuilder.CreateObject("setting_info", mutiClickGroupObj.transform);
        carouselStyle.ValueTextRect.Apply(settingInfoObj.GetComponent<RectTransform>());

        var nowsetionObj = RowElementBuilder.CreateObject("nowsetion", settingInfoObj.transform);
        var nowsetionBg = RowElementBuilder.AddImage(nowsetionObj, carouselStyle.DotSprite, carouselStyle.DotType, new Color(1f, 1f, 1f, 0.1f));
        nowsetionBg.raycastTarget = false;

        var textObj = RowElementBuilder.CreateObject("Text", nowsetionObj.transform);
        RowElementBuilder.SetRect(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var valueText = RowElementBuilder.AddText(textObj, carouselStyle.ValueTextAppearance, string.Empty);
        valueText.raycastTarget = false;

        var dotGroupObj = RowElementBuilder.CreateObject("Toggle_group", settingInfoObj.transform);
        carouselStyle.DotGroupRect.Apply(dotGroupObj.GetComponent<RectTransform>());

        var dotHlg = dotGroupObj.AddComponent<HorizontalLayoutGroup>();
        dotHlg.spacing = 5f;
        dotHlg.padding = new RectOffset(10, 10, 0, 0);
        dotHlg.childAlignment = TextAnchor.LowerCenter;
        dotHlg.childControlWidth = false;
        dotHlg.childControlHeight = false;
        dotHlg.childForceExpandWidth = false;
        dotHlg.childForceExpandHeight = false;

        VanillaComponentApplier.ApplyToRow(root.transform, previousButton);

        return root.transform;
    }

    private static M1Button BuildArrowButton(string name, RectData rect, CarouselStyle style, Transform parent, bool flipHorizontal)
    {
        var buttonObj = RowElementBuilder.CreateObject(name, parent);
        rect.Apply(buttonObj.GetComponent<RectTransform>());

        var image = RowElementBuilder.AddImage(buttonObj, style.ArrowSprite, style.ArrowType, style.ArrowColor);
        image.raycastTarget = false;

        if (flipHorizontal)
        {
            var imageRect = image.GetComponent<RectTransform>();
            imageRect.localScale = new Vector3(-1f, 1f, 1f);
        }

        var button = buttonObj.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f),
            pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f),
            disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f,
        };
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(buttonObj.transform, addDySelect: true, addAudio: true);

        return button;
    }

    internal static GameObject CreateDot(CarouselStyle style, int index, Transform parent)
    {
        var dotObj = RowElementBuilder.CreateObject(index.ToString(), parent);
        style.DotRect.Apply(dotObj.GetComponent<RectTransform>());
        var dotImage = RowElementBuilder.AddImage(dotObj, style.DotSprite, style.DotType, style.UnselectedDotColor);
        dotImage.raycastTarget = false;

        var layout = dotObj.AddComponent<LayoutElement>();
        layout.minWidth = style.DotRect.SizeDelta.x;
        layout.minHeight = style.DotRect.SizeDelta.y;
        layout.preferredWidth = style.DotRect.SizeDelta.x;
        layout.preferredHeight = style.DotRect.SizeDelta.y;

        return dotObj;
    }
}
