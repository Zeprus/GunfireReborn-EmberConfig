namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using SettingsLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct TabStyle(
    TextAppearance Selected,
    TextAppearance Unselected,
    float Width,
    float Height)
{
    public Sprite? SelectedBackgroundSprite { get; init; }
    public RectData? SelectedBackgroundRect { get; init; }
    public uint ClickSoundEventId { get; init; }

    internal static TabStyle? Capture(Transform panelRoot, TextAppearance fallback)
    {
        var tabSwitch = TransformFinder.Find(panelRoot, "tab_switch");
        if (tabSwitch is null)
            return null;

        var vanillaTabs = new List<(Transform tab, M1Toggle toggle)>();
        for (int i = 0; i < tabSwitch.childCount; i++)
        {
            var child = tabSwitch.GetChild(i);
            if (child.name.StartsWith("tab_custom_", StringComparison.OrdinalIgnoreCase))
                continue;
            if (child.GetComponent<M1Toggle>() is M1Toggle toggle)
                vanillaTabs.Add((child, toggle));
        }

        if (vanillaTabs.Count == 0)
            return null;

        Transform? selectedTab = null;
        Transform? unselectedTab = null;
        foreach (var (tab, toggle) in vanillaTabs)
        {
            if (toggle.isOn)
                selectedTab = tab;
            else
                unselectedTab ??= tab;
        }

        // Prefer a selected tab for the selected text color and an unselected tab for the gray color.
        var referenceTab = selectedTab ?? vanillaTabs[0].tab;
        var unselectedReferenceTab = unselectedTab ?? referenceTab;

        var tabRect = referenceTab.GetComponent<RectTransform>();
        var tabWidth = tabRect is not null ? tabRect.sizeDelta.x : 220f;
        var tabHeight = tabRect is not null ? tabRect.sizeDelta.y : 60f;

        var selectedText = referenceTab.Find("type_name")?.GetComponent<TextMeshProUGUI>();
        var unselectedText = unselectedReferenceTab.Find("type_name")?.GetComponent<TextMeshProUGUI>();
        var referenceText = selectedText ?? unselectedText;
        if (referenceText is null)
            return null;

        var selectedAppearance = selectedText is not null
            ? TextAppearance.From(selectedText, 30f)
            : fallback with { Color = new Color(0.871f, 0.792f, 0.592f, 1f) };

        var unselectedAppearance = unselectedText is not null
            ? TextAppearance.From(unselectedText, 30f)
            : fallback with { Color = new Color(0.416f, 0.408f, 0.392f, 1f) };

        var checkmarkTransform = referenceTab.Find("Background/Checkmark");
        var checkmarkImage = checkmarkTransform?.GetComponent<Image>();
        var selectedBackgroundSprite = checkmarkImage?.sprite;
        RectData? selectedBackgroundRect = checkmarkTransform is not null
            ? RectData.From(checkmarkTransform.GetComponent<RectTransform>())
            : null;

        var akEvent = referenceTab.GetComponent<AkEvent>();
        var clickSoundEventId = akEvent?.data?.Id ?? 0u;

        return new TabStyle(
            selectedAppearance,
            unselectedAppearance,
            tabWidth > 0 ? tabWidth : 220f,
            tabHeight > 0 ? tabHeight : 60f)
        {
            SelectedBackgroundSprite = selectedBackgroundSprite,
            SelectedBackgroundRect = selectedBackgroundRect,
            ClickSoundEventId = clickSoundEventId,
        };
    }

    internal static TabStyle Fallback(TextAppearance title) =>
        new(
            title with { Color = new Color(0.871f, 0.792f, 0.592f, 1f), FontSize = 30f },
            title with { Color = new Color(0.416f, 0.408f, 0.392f, 1f), FontSize = 30f },
            220f,
            60f)
        {
            SelectedBackgroundSprite = null,
        };
}
