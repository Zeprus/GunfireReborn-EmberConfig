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
    private TextMeshProUGUI? titleText;
    private TextMeshProUGUI? descriptionText;

    private static readonly ColorBlock RedColorBlock = new()
    {
        normalColor = new Color(0.25f, 0.05f, 0.05f, 0.42f),
        highlightedColor = new Color(0.40f, 0.12f, 0.12f, 0.50f),
        pressedColor = new Color(0.20f, 0.04f, 0.04f, 0.42f),
        disabledColor = new Color(0.15f, 0.03f, 0.03f, 0.30f),
        colorMultiplier = 1f,
        fadeDuration = 0.1f
    };

    public void Show(Transform viewport, StyleCatalog style, string title, string description, Action onReset, Action onCancel)
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

        // Hide the vanilla text objects so they do not overlap with our own.
        SetVanillaTextActive(bg, false);

        EnsureTextObjects(bg, style);

        if (titleText is not null)
        {
            titleText.gameObject.SetActive(true);
            titleText.text = title;
            titleText.ForceMeshUpdate(true);
        }

        if (descriptionText is not null)
        {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = description;
            descriptionText.ForceMeshUpdate(true);
        }

        EnsureContainer(bg);
        if (container is null)
            return;

        container.SetActive(true);

        // Clear any previously created buttons.
        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            var child = container.transform.GetChild(i);
            if (child is not null)
                UnityEngine.Object.Destroy(child.gameObject);
        }

        var cancelTextStyle = style.Row.Title;
        var resetTextStyle = style.Row.Title;
        Sprite? sprite = style.Row.BackgroundSprite;
        var imageType = style.Row.BackgroundType;
        var cancelImageColor = Color.white;
        var cancelColors = RedColorBlock;

        if (style.KeybindButton is { } keybindStyle)
        {
            cancelTextStyle = keybindStyle.Text;
            sprite = keybindStyle.BackgroundSprite;
            imageType = keybindStyle.BackgroundType;
            cancelImageColor = keybindStyle.BackgroundColor;
            cancelColors = keybindStyle.ButtonColors;
        }

        CreateButton(container.transform, resetTextStyle, sprite, imageType, Color.white, RedColorBlock, "Reset", onReset);
        CreateButton(container.transform, cancelTextStyle, sprite, imageType, cancelImageColor, cancelColors, "Cancel", onCancel);

        Canvas.ForceUpdateCanvases();
    }

    public void Hide()
    {
        if (titleText is not null && !titleText.Equals(null))
            titleText.gameObject.SetActive(false);

        if (descriptionText is not null && !descriptionText.Equals(null))
            descriptionText.gameObject.SetActive(false);

        if (container is not null && !container.Equals(null))
            container.SetActive(false);

        if (mask is not null && !mask.Equals(null))
        {
            var bg = mask.transform.Find("bg");
            if (bg is not null)
                SetVanillaTextActive(bg, true);

            SetLogicEnabled(true);
            mask.SetActive(false);
        }

        SettingsPanelState.IsBlockingClose = false;
    }

    private void EnsureTextObjects(Transform bg, StyleCatalog style)
    {
        if (titleText is null || titleText.Equals(null))
        {
            titleText = CreateTextObject(bg, "ConfirmationTitleText", style.Row.Title, 26f);
            SetTopAnchors(titleText.rectTransform, -30f, 40f);
        }

        if (descriptionText is null || descriptionText.Equals(null))
        {
            descriptionText = CreateTextObject(bg, "ConfirmationDescriptionText", style.Row.Title, 20f);
            SetTopAnchors(descriptionText.rectTransform, -80f, 50f);
        }
    }

    private static TextMeshProUGUI CreateTextObject(Transform parent, string name, TextAppearance appearance, float fontSize)
    {
        var go = new GameObject(name);
        var rect = (RectTransform)go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        go.AddComponent<CanvasRenderer>();
        var text = go.AddComponent<TextMeshProUGUI>();
        TextAppearanceApplier.Apply(text, appearance);
        text.fontSize = fontSize;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.color = appearance.Color;
        text.margin = Vector4.zero;

        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        go.SetActive(true);
        return text;
    }

    private static void SetTopAnchors(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0.1f, 1f);
        rect.anchorMax = new Vector2(0.9f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, height);
        rect.anchoredPosition = new Vector2(0f, y);
    }

    private static void SetVanillaTextActive(Transform bg, bool active)
    {
        if (bg.Find("bg_txt")?.gameObject is GameObject bgText)
            bgText.SetActive(active);
        else if (bg.Find("key_txt")?.gameObject is GameObject keyText)
            keyText.SetActive(active);

        if (bg.Find("bg_tip")?.gameObject is GameObject bgTip)
            bgTip.SetActive(active);
        else if (bg.Find("key_tip")?.gameObject is GameObject keyTip)
            keyTip.SetActive(active);
    }

    private void EnsureContainer(Transform bg)
    {
        if (container is not null && !container.Equals(null))
            return;

        container = new GameObject("ConfirmationButtons");
        var rect = (RectTransform)container.AddComponent<RectTransform>();
        rect.SetParent(bg, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 70f);
        rect.anchoredPosition = new Vector2(0f, 80f);

        var layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 30f;
        layout.padding = new RectOffset(40, 40, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childAlignment = TextAnchor.MiddleCenter;

        var layoutElement = container.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
    }

    private static M1Button CreateButton(Transform parent, TextAppearance textStyle, Sprite? sprite, Image.Type type,
        Color imageColor, ColorBlock colors, string label, Action onClick)
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
        layout.minWidth = 120f;
        layout.preferredWidth = 160f;
        layout.minHeight = 45f;
        layout.preferredHeight = 45f;
        layout.flexibleWidth = 1f;
        layout.flexibleHeight = 1f;

        var textObj = RowElementBuilder.CreateObject("Text", go.transform);
        RowElementBuilder.SetRect(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var text = RowElementBuilder.AddText(textObj, textStyle, label, TextAlignmentOptions.Center);
        text.raycastTarget = false;

        var button = go.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
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
