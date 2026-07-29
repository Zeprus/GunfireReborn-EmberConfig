namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual style for a vanilla two-option "Switch" boolean control.
/// </summary>
internal readonly record struct SwitchStyle(
    Color OptionBackgroundColor,
    Color OptionCheckmarkColor,
    Sprite? OptionBackgroundSprite,
    Sprite? OptionCheckmarkSprite,
    Image.Type OptionBackgroundType,
    Image.Type OptionCheckmarkType,
    TextAppearance LabelTextAppearance,
    RectData ClickGroupRect,
    Vector2 OptionSize,
    float Spacing,
    TextAnchor ChildAlignment)
{
    public string Option1Label { get; init; } = "On";
    public string Option2Label { get; init; } = "Off";
    public uint ClickSoundEventId { get; init; }

    internal static SwitchStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        var clickGroup = FindClickGroup(panelRoot);
        if (clickGroup is null)
            return null;

        var hlg = clickGroup.GetComponent<HorizontalLayoutGroup>();
        var clickGroupRect = RectData.From(clickGroup.GetComponent<RectTransform>());
        var spacing = hlg?.spacing ?? 40f;
        var childAlignment = hlg?.childAlignment ?? TextAnchor.MiddleCenter;

        var option1 = clickGroup.GetChild(0);
        var option2 = clickGroup.childCount > 1 ? clickGroup.GetChild(1) : null;
        if (option1 is null || option2 is null)
            return null;

        var optionSize = option1.GetComponent<RectTransform>().sizeDelta;

        var background1 = option1.Find("Background")?.GetComponent<Image>();
        var checkmark1 = option1.Find("Background/Checkmark")?.GetComponent<Image>()
            ?? option1.Find("Checkmark")?.GetComponent<Image>();
        var label1 = option1.Find("Label")?.GetComponent<TextMeshProUGUI>();

        var bgColor = background1?.color ?? new Color(0.067f, 0.067f, 0.067f, 1f);
        var checkColor = checkmark1?.color ?? new Color(1f, 1f, 1f, 0.102f);
        var bgSprite = background1?.sprite ?? fallbackSprite;
        var checkSprite = checkmark1?.sprite ?? fallbackSprite;
        var bgType = background1?.type ?? Image.Type.Sliced;
        var checkType = checkmark1?.type ?? Image.Type.Simple;
        var textAppearance = label1 is not null ? TextAppearance.From(label1) : fallbackText;

        return new SwitchStyle(
            bgColor,
            checkColor,
            bgSprite,
            checkSprite,
            bgType,
            checkType,
            textAppearance,
            clickGroupRect,
            optionSize,
            spacing,
            childAlignment)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(clickGroup),
        };
    }

    private static Transform? FindClickGroup(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (!string.Equals(t.name, "ClickGroup", StringComparison.OrdinalIgnoreCase))
                continue;

            if (t.GetComponent<ToggleGroup>() is null)
                continue;

            var toggles = t.GetComponentsInChildren<Toggle>(true);
            if (toggles.Length >= 2)
                return t;
        }

        return null;
    }
}
