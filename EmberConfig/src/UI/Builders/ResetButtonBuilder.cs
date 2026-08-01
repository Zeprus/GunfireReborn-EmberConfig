namespace EmberConfig.UI;

using System;
using DYControl;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and maintains the "Reset Visibility" button at the bottom of the EmberConfig tab.
/// </summary>
internal static class ResetButtonBuilder
{
    private const string ButtonName = "VisibilityResetButton";
    private const string SpacerName = "VisibilityResetSpacer";

    public static Transform Ensure(Transform content, StyleCatalog style, Action onReset, ConfirmationCoverMask? confirmation = null)
    {
        var existingButton = content.Find(ButtonName);
        var existingSpacer = content.Find(SpacerName);

        Transform button;
        Transform? spacer;
        if (existingButton is not null)
        {
            button = existingButton;
            WireClick(button, content, style, onReset, confirmation);
            spacer = existingSpacer;
        }
        else
        {
            button = CreateResetButton(content, style, onReset, confirmation);
            spacer = null;
        }

        if (spacer is null)
        {
            var spacerGo = new GameObject(SpacerName);
            var spacerRect = (RectTransform)spacerGo.AddComponent<RectTransform>();
            spacerRect.SetParent(content, false);

            var spacerLayout = spacerGo.AddComponent<LayoutElement>();
            spacerLayout.minHeight = 30f;
            spacerLayout.preferredHeight = 30f;

            spacer = spacerGo.transform;
        }

        spacer.SetAsLastSibling();
        button.SetAsLastSibling();

        return button;
    }

    private static Transform CreateResetButton(Transform content, StyleCatalog style, Action onReset, ConfirmationCoverMask? confirmation)
    {
        var go = new GameObject(ButtonName);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(content, false);
        style.Row.RowRect.Apply(rect);

        _ = go.AddComponent<CanvasRenderer>();

        var image = go.AddComponent<Image>();
        image.sprite = style.Row.BackgroundSprite;
        image.type = style.Row.BackgroundType;
        image.color = Color.white;

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = style.Row.Height;
        layout.flexibleWidth = 1f;

        var textObj = RowElementBuilder.CreateObject("Text", go.transform);
        RowElementBuilder.SetRect(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        RowElementBuilder.AddText(textObj, style.Row.Title, "Reset Visibility", TextAlignmentOptions.Center);

        var button = go.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = UIStyleConstants.DestructiveColorBlock;
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(go.transform);

        var dySelect = go.GetComponent<DYSelect>();
        if (dySelect is not null)
            dySelect.isCurBtn = true;

        WireClick(go.transform, content, style, onReset, confirmation);

        go.SetActive(true);
        button.TranslateState(0, true);

        return go.transform;
    }

    private static void WireClick(Transform buttonTransform, Transform content, StyleCatalog style, Action onReset, ConfirmationCoverMask? confirmation)
    {
        var button = buttonTransform.GetComponent<M1Button>();
        if (button is null)
            return;

        button.onClick.RemoveAllListeners();

        Action onResetClick = () => OnResetClick(content, style, onReset, confirmation);
        button.onClick.AddListener(onResetClick);
    }

    private static void OnResetClick(Transform content, StyleCatalog style, Action onReset, ConfirmationCoverMask? confirmation)
    {
        var viewport = content.parent ?? PanelLocator.FindPanelRoot(content)?.Find("bg_windows/setting_scroll/Viewport");
        if (confirmation is not null && viewport is not null)
        {
            Action confirm = () =>
            {
                confirmation.Hide();
                onReset();
            };

            Action cancel = () => confirmation.Hide();

            confirmation.Show(viewport, style,
                "Reset all visibility settings?",
                "This will delete every mod visibility setting and restore the default visible state.",
                confirm, cancel);
        }
        else
        {
            onReset();
        }
    }
}
