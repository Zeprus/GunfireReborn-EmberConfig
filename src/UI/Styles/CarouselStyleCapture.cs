namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class CarouselStyleCapture
{
    internal static CarouselStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        var mutiClickGroup = FindMutiClickGroup(panelRoot);
        if (mutiClickGroup is null)
            return CarouselStyle.Default(fallbackSprite, fallbackText);

        var previous = FindChild(mutiClickGroup, "previous");
        var next = FindChild(mutiClickGroup, "next");
        var settingInfo = FindChild(mutiClickGroup, "setting_info");

        var previousArrow = previous is not null ? FindChild(previous, "Image")?.GetComponent<Image>() : null;
        var nextArrow = next is not null ? FindChild(next, "Image")?.GetComponent<Image>() : null;
        var arrowSprite = previousArrow?.sprite ?? nextArrow?.sprite ?? fallbackSprite;
        var arrowColor = previousArrow?.color ?? nextArrow?.color ?? new Color(1f, 1f, 1f, 0.5f);
        var arrowType = previousArrow?.type ?? nextArrow?.type ?? Image.Type.Simple;

        var nowsetion = settingInfo is not null ? FindChild(settingInfo, "nowsetion") : null;
        var valueText = nowsetion is not null ? nowsetion.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        var valueTextAppearance = valueText is not null ? TextAppearance.From(valueText) : fallbackText with
        {
            Color = new Color(0.584f, 0.518f, 0.341f, 1f),
            FontSize = 18f,
            Alignment = TextAlignmentOptions.Center,
        };

        var toggleGroup = settingInfo is not null ? FindChild(settingInfo, "Toggle_group") : null;
        var dot = toggleGroup is not null && toggleGroup.childCount > 0 ? FindChild(toggleGroup.GetChild(0), "Background")?.GetComponent<Image>() : null;
        var dotSprite = dot?.sprite ?? fallbackSprite;
        var dotType = dot?.type ?? Image.Type.Simple;

        return new CarouselStyle(
            arrowSprite,
            arrowColor,
            arrowType,
            dotSprite,
            dotType,
            new Color(0.584f, 0.518f, 0.341f, 1f),
            dot?.color ?? new Color(0.4f, 0.4f, 0.4f, 1f),
            valueTextAppearance,
            RectData.From(mutiClickGroup.GetComponent<RectTransform>()),
            RectData.From(previous?.GetComponent<RectTransform>() ?? mutiClickGroup.GetComponent<RectTransform>()),
            RectData.From(next?.GetComponent<RectTransform>() ?? mutiClickGroup.GetComponent<RectTransform>()),
            RectData.From(nowsetion?.GetComponent<RectTransform>() ?? mutiClickGroup.GetComponent<RectTransform>()),
            RectData.From(toggleGroup?.GetComponent<RectTransform>() ?? mutiClickGroup.GetComponent<RectTransform>()),
            RectData.From(dot?.GetComponent<RectTransform>() ?? (toggleGroup?.GetChild(0)?.GetComponent<RectTransform>() ?? mutiClickGroup.GetComponent<RectTransform>())))
        {
            ClickSoundEventId = WwiseAudio.GetEventId(mutiClickGroup),
        };
    }

    private static Transform? FindMutiClickGroup(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (string.Equals(t.name, "MutiClickGroup", StringComparison.OrdinalIgnoreCase))
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
}
