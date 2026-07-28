namespace SettingsLib.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class KeybindButtonStyleCapture
{
    internal static KeybindButtonStyle? Capture(Transform panelRoot, TextAppearance fallback, Sprite? rowSprite)
    {
        var primaryButton = FindButtonByName(panelRoot, "change_button_1");
        var secondaryButton = FindButtonByName(panelRoot, "change_button_2");
        var anyButton = primaryButton ?? secondaryButton;
        if (anyButton is null)
            return null;

        var boundText = FindBoundText(panelRoot, anyButton.parent);
        var unboundText = FindUnboundText(panelRoot);

        if (boundText is null && unboundText is null)
            return null;

        var primaryRect = primaryButton?.GetComponent<RectTransform>() ?? anyButton.GetComponent<RectTransform>();
        var primaryRectData = primaryRect is not null ? RectData.From(primaryRect) : KeybindButtonStyle.DefaultPrimaryRect;

        var primaryImage = primaryButton?.GetComponent<Image>() ?? anyButton.GetComponent<Image>();
        var backgroundColor = primaryImage?.color ?? new Color(0.067f, 0.067f, 0.067f, 1f);
        var backgroundSprite = primaryImage?.sprite ?? rowSprite;
        var backgroundType = primaryImage?.type ?? Image.Type.Sliced;

        var vanillaButton = anyButton.GetComponent<Button>();
        var buttonColors = vanillaButton?.colors ?? new ColorBlock();

        var secondaryRect = secondaryButton?.GetComponent<RectTransform>();
        var secondaryRectData = secondaryRect is not null ? RectData.From(secondaryRect) : KeybindButtonStyle.DefaultSecondaryRect;

        var item = anyButton.parent?.parent;
        var itemRect = item?.GetComponent<RectTransform>();
        var itemRectData = itemRect is not null ? RectData.From(itemRect) : KeybindButtonStyle.DefaultItemRect;

        var effectiveBoundText = boundText ?? unboundText!;
        var effectiveNoneText = unboundText ?? boundText!;

        return new KeybindButtonStyle(
            TextAppearance.From(effectiveBoundText, 20f),
            TextAppearance.From(effectiveNoneText, 20f),
            effectiveBoundText.spriteAsset,
            backgroundColor,
            backgroundSprite,
            backgroundType,
            buttonColors,
            primaryRectData,
            secondaryRectData,
            itemRectData)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(anyButton),
        };
    }

    private static Transform? FindButtonByName(Transform root, string name)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (string.Equals(transforms[i].name, name, StringComparison.OrdinalIgnoreCase))
                return transforms[i];
        }
        return null;
    }

    private static TextMeshProUGUI? FindBoundText(Transform root, Transform? fallbackItem)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        TextMeshProUGUI? best = null;

        foreach (var t in transforms)
        {
            if (!t.name.StartsWith("change_button", StringComparison.OrdinalIgnoreCase))
                continue;

            var texts = t.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                if (!IsSpriteText(tmp))
                    continue;

                if (best is null)
                    best = tmp;
                else if (tmp.transform.parent?.gameObject.activeInHierarchy == true &&
                         best.transform.parent?.gameObject.activeInHierarchy != true)
                    best = tmp;
            }
        }

        return best ?? FindSpriteText(fallbackItem);
    }

    private static TextMeshProUGUI? FindUnboundText(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        TextMeshProUGUI? best = null;

        foreach (var t in transforms)
        {
            if (!t.name.StartsWith("change_button", StringComparison.OrdinalIgnoreCase))
                continue;

            var texts = t.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                if (!IsNoneText(tmp))
                    continue;

                if (best is null || IsBolder(tmp, best))
                    best = tmp;
            }
        }

        return best;
    }

    private static TextMeshProUGUI? FindSpriteText(Transform? item)
    {
        if (item is null)
            return null;

        var texts = item.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (IsSpriteText(texts[i]))
                return texts[i];
        }

        return null;
    }

    private static bool IsSpriteText(TextMeshProUGUI tmp) =>
        tmp.text.StartsWith("<sprite", StringComparison.OrdinalIgnoreCase);

    private static bool IsNoneText(TextMeshProUGUI tmp) =>
        string.Equals(tmp.text, "None", StringComparison.OrdinalIgnoreCase);

    private static bool IsBolder(TextMeshProUGUI candidate, TextMeshProUGUI current)
    {
        var candidateName = candidate.font?.name ?? string.Empty;
        var currentName = current.font?.name ?? string.Empty;
        bool candidateBold = IsBoldName(candidateName);
        bool currentBold = IsBoldName(currentName);
        return candidateBold && !currentBold;
    }

    private static bool IsBoldName(string fontName)
    {
        if (fontName.Contains("Black", StringComparison.OrdinalIgnoreCase))
            return true;
        if (fontName.Contains("Bold", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
