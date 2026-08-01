namespace EmberConfig.UI;

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class GroupHeaderStyleCapture
{
    internal static GroupHeaderStyle? Capture(Transform panelRoot, TextAppearance fallback)
    {
        var (titleContainer, groupTitle) = FindGroupTitle(panelRoot);
        if (groupTitle is null || titleContainer is null)
            return null;

        var subGroupHeight = CaptureSubGroupHeaderHeight(groupTitle);
        var (titlePadding, titleSpacing) = CaptureTitleLayout(titleContainer);
        var headerTextRect = RectData.From(groupTitle.rectTransform);
        var (dividerRect, dividerColor, dividerSprite, dividerType) = CaptureDivider(titleContainer);

        var spacing = CaptureGroupSpacing(panelRoot);

        return new GroupHeaderStyle(
            TextAppearance.From(groupTitle, 24f),
            spacing,
            subGroupHeight,
            titlePadding,
            titleSpacing,
            headerTextRect,
            dividerRect,
            dividerColor,
            dividerSprite,
            dividerType);
    }

    private static float CaptureSubGroupHeaderHeight(TextMeshProUGUI groupTitle)
    {
        var titleTransform = groupTitle.rectTransform;
        var titleLayout = titleTransform.GetComponent<LayoutElement>();
        if (titleLayout is not null && titleLayout.preferredHeight > 0f)
            return titleLayout.preferredHeight;

        if (titleTransform.sizeDelta.y > 0f)
            return titleTransform.sizeDelta.y;

        return 30f;
    }

    private static (RectOffset Padding, float Spacing) CaptureTitleLayout(Transform? titleContainer)
    {
        if (titleContainer is null)
            return (new RectOffset(20, 20, 10, 10), 0f);

        var layout = titleContainer.GetComponent<VerticalLayoutGroup>();
        if (layout is null)
            return (new RectOffset(20, 20, 10, 10), 0f);

        var padding = new RectOffset(layout.padding.left, layout.padding.right, layout.padding.top, layout.padding.bottom);
        return (padding, layout.spacing);
    }

    private static float CaptureGroupSpacing(Transform panelRoot)
    {
        var groupContainers = panelRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in groupContainers)
        {
            if (!string.Equals(t.name, "settion_group_1", StringComparison.OrdinalIgnoreCase))
                continue;

            var layout = t.GetComponent<VerticalLayoutGroup>();
            if (layout is not null)
                return layout.spacing;
        }

        return 10f;
    }

    private static (RectData Rect, Color Color, Sprite? Sprite, Image.Type Type) CaptureDivider(Transform? titleContainer)
    {
        if (titleContainer is null)
            return DefaultDivider();

        Image? bestDivider = null;
        for (int i = 0; i < titleContainer.childCount; i++)
        {
            var child = titleContainer.GetChild(i);
            if (child.GetComponent<TextMeshProUGUI>() is not null)
                continue;

            var image = child.GetComponent<Image>();
            if (image is null || image.sprite is null)
                continue;

            if (IsDividerImage(image))
            {
                bestDivider = image;
                break;
            }

            bestDivider ??= image;
        }

        if (bestDivider is not null)
        {
            var rect = bestDivider.GetComponent<RectTransform>();
            var rectData = rect is not null ? RectData.From(rect) : GroupHeaderStyle.DefaultDividerRect;
            return (rectData, bestDivider.color, bestDivider.sprite, bestDivider.type);
        }

        return DefaultDivider();
    }

    private static bool IsDividerImage(Image image)
    {
        var spriteName = image.sprite.name;
        if (spriteName.Contains("line", StringComparison.OrdinalIgnoreCase))
            return true;

        var rect = image.GetComponent<RectTransform>();
        if (rect is not null && rect.sizeDelta.y is > 0f and < 10f)
            return true;

        if (image.color.a < 1f && image.type == Image.Type.Simple)
            return true;

        return false;
    }

    private static (RectData Rect, Color Color, Sprite? Sprite, Image.Type Type) DefaultDivider() =>
        (GroupHeaderStyle.DefaultDividerRect, new Color(0.584f, 0.518f, 0.341f, 0.3f), null, Image.Type.Sliced);

    private static (Transform? Container, TextMeshProUGUI? Text) FindGroupTitle(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        TextMeshProUGUI? fallback = null;

        foreach (var t in transforms)
        {
            if (!string.Equals(t.name, "title", StringComparison.OrdinalIgnoreCase))
                continue;

            var text = t.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text is null)
                continue;

            fallback ??= text;

            var parentName = t.parent?.name ?? string.Empty;
            if (parentName.StartsWith("settion_group", StringComparison.OrdinalIgnoreCase) && t.gameObject.activeInHierarchy)
                return (t, text);
        }

        return (fallback?.transform, fallback);
    }
}
