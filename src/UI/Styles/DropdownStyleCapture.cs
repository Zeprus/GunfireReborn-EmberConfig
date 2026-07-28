namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class DropdownStyleCapture
{
    internal static DropdownStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        var dropdown = FindDropdownTransform(panelRoot);
        if (dropdown is null)
            return null;

        var itemFallback = DropdownItemStyle.Default(fallbackSprite, fallbackText);
        var templateFallback = DropdownTemplateStyle.Default(fallbackSprite, fallbackText);
        var scrollbarFallback = DropdownScrollbarStyle.Default(fallbackSprite);

        var label = FindChild(dropdown, "Label");
        var labelTmp = label?.GetComponent<TextMeshProUGUI>();
        var arrow = FindChild(dropdown, "Arrow");
        var arrowImage = arrow?.GetComponent<Image>();
        var itemImage = dropdown.GetComponent<Image>();

        var itemStyle = new DropdownItemStyle(
            itemImage?.sprite ?? itemFallback.ItemSprite,
            itemImage?.color ?? itemFallback.ItemColor,
            itemImage?.type ?? itemFallback.ItemType,
            RectData.From(dropdown.GetComponent<RectTransform>()),
            RectOrDefault(label, itemFallback.LabelRect),
            labelTmp is not null ? TextAppearance.From(labelTmp) : itemFallback.LabelTextAppearance,
            labelTmp?.alignment ?? itemFallback.LabelAlignment,
            RectOrDefault(arrow, itemFallback.ArrowRect),
            arrowImage?.sprite ?? itemFallback.ArrowSprite,
            arrowImage?.color ?? itemFallback.ArrowColor,
            arrowImage?.type ?? itemFallback.ArrowType);

        var template = FindChild(dropdown, "Template");
        var templateImage = template?.GetComponent<Image>();
        var viewport = FindChild(template, "Viewport");
        var content = FindChild(viewport, "Content");
        var templateItem = FindChild(content, "Item");
        var templateHighlight = FindChild(templateItem, "Image");
        var templateHighlightImage = templateHighlight?.GetComponent<Image>();
        var itemBg = FindChild(templateItem, "Item Background");
        var itemBgImage = itemBg?.GetComponent<Image>();
        var itemCheck = FindChild(templateItem, "Item Checkmark");
        var itemCheckImage = itemCheck?.GetComponent<Image>();
        var itemLabel = FindChild(templateItem, "Item Label");
        var itemLabelTmp = itemLabel?.GetComponent<TextMeshProUGUI>();

        var templateStyle = new DropdownTemplateStyle(
            RectOrDefault(template, templateFallback.TemplateRect),
            templateImage?.sprite ?? templateFallback.TemplateSprite,
            templateImage?.color ?? templateFallback.TemplateBgColor,
            templateImage?.type ?? templateFallback.TemplateImageType,
            RectOrDefault(viewport, templateFallback.ViewportRect),
            RectOrDefault(content, templateFallback.ContentRect),
            RectOrDefault(templateItem, templateFallback.TemplateItemRect),
            RectOrDefault(templateHighlight, templateFallback.TemplateHighlightRect),
            templateHighlightImage?.sprite ?? templateFallback.TemplateHighlightSprite,
            templateHighlightImage?.color ?? templateFallback.TemplateHighlightColor,
            templateHighlightImage?.type ?? templateFallback.TemplateHighlightType,
            RectOrDefault(itemBg, templateFallback.ItemBackgroundRect),
            itemBgImage?.sprite ?? templateFallback.ItemBgSprite,
            itemBgImage?.color ?? templateFallback.ItemBgColor,
            itemBgImage?.type ?? templateFallback.ItemBgType,
            RectOrDefault(itemCheck, templateFallback.ItemCheckmarkRect),
            itemCheckImage?.sprite ?? templateFallback.ItemCheckmarkSprite,
            itemCheckImage?.color ?? templateFallback.ItemCheckmarkColor,
            itemCheckImage?.type ?? templateFallback.ItemCheckmarkType,
            RectOrDefault(itemLabel, templateFallback.ItemLabelRect),
            itemLabelTmp is not null ? TextAppearance.From(itemLabelTmp) : templateFallback.ItemLabelTextAppearance,
            itemLabelTmp?.alignment ?? templateFallback.ItemLabelAlignment);

        var scrollbar = FindChild(template, "Scrollbar");
        var scrollbarImage = scrollbar?.GetComponent<Image>();
        var slidingArea = FindChild(scrollbar, "Sliding Area");
        var handle = FindChild(slidingArea, "Handle");
        var handleImage = handle?.GetComponent<Image>();

        var scrollbarStyle = new DropdownScrollbarStyle(
            RectOrDefault(scrollbar, scrollbarFallback.ScrollbarRect),
            scrollbarImage?.sprite ?? scrollbarFallback.ScrollbarSprite,
            scrollbarImage?.color ?? scrollbarFallback.ScrollbarColor,
            scrollbarImage?.type ?? scrollbarFallback.ScrollbarType,
            RectOrDefault(slidingArea, scrollbarFallback.SlidingAreaRect),
            RectOrDefault(handle, scrollbarFallback.HandleRect),
            handleImage?.sprite ?? scrollbarFallback.HandleSprite,
            handleImage?.color ?? scrollbarFallback.HandleColor,
            handleImage?.type ?? scrollbarFallback.HandleType);

        return new DropdownStyle(itemStyle, templateStyle, scrollbarStyle)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(dropdown),
        };
    }

    private static Transform? FindDropdownTransform(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (t.GetComponent<TMP_Dropdown>() is not null)
                return t;
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (string.Equals(t.name, "Dropdown", StringComparison.OrdinalIgnoreCase))
                return t;
        }

        return null;
    }

    private static Transform? FindChild(Transform? parent, string name)
    {
        if (parent is null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                return child;
        }
        return null;
    }

    private static RectData RectOrDefault(Transform? target, RectData fallback) =>
        target is not null
            ? RectData.From(target.GetComponent<RectTransform>())
            : fallback;
}
