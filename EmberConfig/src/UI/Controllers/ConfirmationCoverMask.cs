namespace EmberConfig.UI;

using System;
using DYControl;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reuses the vanilla settings cover mask to show a two-button confirmation
/// dialog styled like the keybind capture prompt.
/// </summary>
internal sealed class ConfirmationCoverMask
{
    private GameObject? mask;
    private GameObject? container;

    /// <summary>
    /// Shows a confirmation dialog over the settings panel.
    /// </summary>
    /// <param name="viewport">The settings viewport (used to locate the cover mask).</param>
    /// <param name="style">The captured UI style catalog.</param>
    /// <param name="message">The main confirmation message.</param>
    /// <param name="tip">The secondary explanatory text.</param>
    /// <param name="onConfirm">Called when the user confirms.</param>
    /// <param name="onCancel">Called when the user cancels.</param>
    public void Show(Transform viewport, StyleCatalog style, string message, string tip, Action onConfirm, Action onCancel)
    {
        if (mask is null || mask.Equals(null))
        {
            mask = viewport.Find("covery_mask")?.gameObject;
            if (mask is null)
                return;
        }

        var bg = mask.transform.Find("bg");
        if (bg is null)
            return;

        SetLogicEnabled(false);
        mask.SetActive(true);
        SettingsPanelState.IsBlockingClose = true;

        if (bg.Find("bg_txt")?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI messageText)
        {
            TextAppearanceApplier.Apply(messageText, style.Row.Title, message, TextAlignmentOptions.Center);
        }
        else if (bg.Find("key_txt")?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI keyText)
        {
            TextAppearanceApplier.Apply(keyText, style.Row.Title, message, TextAlignmentOptions.Center);
        }

        if (bg.Find("bg_tip")?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI tipText)
        {
            TextAppearanceApplier.Apply(tipText, style.Row.Title, tip, TextAlignmentOptions.Center);
        }
        else if (bg.Find("key_tip")?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI keyTip)
        {
            TextAppearanceApplier.Apply(keyTip, style.Row.Title, tip, TextAlignmentOptions.Center);
        }

        if (container is null || container.Equals(null))
        {
            container = new GameObject("ConfirmationButtons");
            var rect = (RectTransform)container.AddComponent<RectTransform>();
            rect.SetParent(bg, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 100f);
            rect.anchoredPosition = new Vector2(0f, 80f);

            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 40f;
            layout.padding = new RectOffset(60, 60, 15, 15);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
        }

        container.SetActive(true);

        // Clear any previously created buttons.
        for (int i = container.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(container.transform.GetChild(i).gameObject);

        var keybindStyle = style.KeybindButton;
        if (keybindStyle.HasValue)
        {
            CreateButton(container.transform, keybindStyle.Value.Text, keybindStyle.Value.BackgroundColor,
                keybindStyle.Value.BackgroundSprite, keybindStyle.Value.BackgroundType,
                keybindStyle.Value.ButtonTransition, keybindStyle.Value.ButtonColors, "Confirm", onConfirm);
            CreateButton(container.transform, keybindStyle.Value.Text, keybindStyle.Value.BackgroundColor,
                keybindStyle.Value.BackgroundSprite, keybindStyle.Value.BackgroundType,
                keybindStyle.Value.ButtonTransition, keybindStyle.Value.ButtonColors, "Cancel", onCancel);
        }
        else
        {
            var fallbackBlock = new ColorBlock
            {
                normalColor = new Color(0.55f, 0.12f, 0.12f, 1f),
                highlightedColor = new Color(0.8f, 0.25f, 0.25f, 1f),
                pressedColor = new Color(0.4f, 0.08f, 0.08f, 1f),
                disabledColor = new Color(0.35f, 0.08f, 0.08f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            CreateButton(container.transform, style.Row.Title, Color.white, style.Row.BackgroundSprite,
                style.Row.BackgroundType, Selectable.Transition.ColorTint, fallbackBlock, "Confirm", onConfirm);
            CreateButton(container.transform, style.Row.Title, Color.white, style.Row.BackgroundSprite,
                style.Row.BackgroundType, Selectable.Transition.ColorTint, fallbackBlock, "Cancel", onCancel);
        }
    }

    public void Hide()
    {
        if (container is not null && !container.Equals(null))
            container.SetActive(false);

        if (mask is not null && !mask.Equals(null))
        {
            SetLogicEnabled(true);
            mask.SetActive(false);
        }

        SettingsPanelState.IsBlockingClose = false;
    }

    private static M1Button CreateButton(Transform parent, TextAppearance textStyle, Color imageColor,
        Sprite? sprite, Image.Type type, Selectable.Transition transition, ColorBlock colors, string label, Action onClick)
    {
        var go = new GameObject($"ConfirmButton_{label}");
        var rect = (RectTransform)go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        _ = go.AddComponent<CanvasRenderer>();

        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = imageColor;

        var layout = go.AddComponent<LayoutElement>();
        layout.minWidth = 150f;
        layout.preferredWidth = 200f;
        layout.minHeight = 50f;
        layout.preferredHeight = 50f;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 1f;

        var textObj = RowElementBuilder.CreateObject("Text", go.transform);
        RowElementBuilder.SetRect(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        RowElementBuilder.AddText(textObj, textStyle, label, TextAlignmentOptions.Center);

        var button = go.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = transition;
        button.colors = colors;
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(go.transform);

        var dySelect = go.GetComponent<DYSelect>();
        if (dySelect is not null)
            dySelect.isCurBtn = true;

        Action handler = () => onClick();
        button.onClick.AddListener(handler);

        go.SetActive(true);
        return button;
    }

    private void SetLogicEnabled(bool enabled)
    {
        if (mask is null || mask.Equals(null))
            return;

        var behaviours = mask.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (b is null || b.Equals(null))
                continue;

            var typeName = b.GetIl2CppType().Name;
            if (typeName is "KeyboardSwapPanel_Logic")
                b.enabled = enabled;
        }
    }
}
