namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class GroupContainerBuilder
{
    internal static Transform Build(string name, string title, RowStyle rowStyle, GroupHeaderStyle groupStyle, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        RowElementBuilder.SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(rowStyle.Width, 0f), new Vector2(rowStyle.Width / 2f, 0f));

        go.AddComponent<CanvasRenderer>();
        var bgImage = go.AddComponent<Image>();
        bgImage.sprite = rowStyle.BackgroundSprite;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = Color.clear;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = groupStyle.Spacing;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var titleObj = RowElementBuilder.CreateObject("title", go.transform);
        RowElementBuilder.SetRect(titleObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(rowStyle.Width, 0f), new Vector2(rowStyle.Width / 2f, 0f));

        var titleLayout = titleObj.AddComponent<VerticalLayoutGroup>();
        titleLayout.spacing = groupStyle.TitleSpacing;
        titleLayout.childForceExpandWidth = true;
        titleLayout.childForceExpandHeight = false;
        titleLayout.childAlignment = TextAnchor.UpperLeft;
        titleLayout.padding = new RectOffset(
            groupStyle.TitlePadding.left,
            groupStyle.TitlePadding.right,
            groupStyle.TitlePadding.top,
            groupStyle.TitlePadding.bottom);

        var titleFitter = titleObj.AddComponent<ContentSizeFitter>();
        titleFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var titleTextObj = RowElementBuilder.CreateObject("Text", titleObj.transform);
        groupStyle.HeaderTextRect.Apply(titleTextObj.GetComponent<RectTransform>());
        var titleText = RowElementBuilder.AddText(titleTextObj, groupStyle.Header, title, TextAlignmentOptions.Left);
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        var dividerObj = RowElementBuilder.CreateObject("Image", titleObj.transform);
        groupStyle.DividerRect.Apply(dividerObj.GetComponent<RectTransform>());
        var dividerImage = RowElementBuilder.AddImage(dividerObj, groupStyle.DividerSprite, groupStyle.DividerType, groupStyle.DividerColor);
        dividerImage.raycastTarget = false;

        var dividerLayout = dividerObj.AddComponent<LayoutElement>();
        dividerLayout.preferredHeight = groupStyle.DividerRect.SizeDelta.y;
        dividerLayout.minHeight = groupStyle.DividerRect.SizeDelta.y;

        go.SetActive(true);
        return go.transform;
    }
}
