namespace SettingsLib.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the visual GameObject for a carousel tab slot.
/// </summary>
internal static class TabCarouselSlotBuilder
{
    internal static TabCarouselSlot Build(
        Transform parent,
        TabStyle style,
        float width,
        float height,
        Vector2 anchoredPosition)
    {
        var go = RowElementBuilder.CreateObject("SL_TabSlot", parent);
        var rect = go.GetComponent<RectTransform>();
        RowElementBuilder.SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(width, height), anchoredPosition);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        go.AddComponent<CanvasRenderer>();

        var canvasGroup = go.AddComponent<CanvasGroup>();

        var hitImage = go.AddComponent<Image>();
        hitImage.color = Color.clear;
        hitImage.raycastTarget = true;

        var button = go.AddComponent<Button>();
        button.targetGraphic = hitImage;
        button.transition = Selectable.Transition.None;
        button.interactable = true;

        var backgroundObj = RowElementBuilder.CreateObject("Background", go.transform);
        RowElementBuilder.SetRect(backgroundObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backgroundObj.SetActive(false);

        var checkmarkObj = RowElementBuilder.CreateObject("Checkmark", backgroundObj.transform);
        var checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
        if (style.SelectedBackgroundRect.HasValue)
        {
            var rectData = style.SelectedBackgroundRect.Value;
            rectData.Apply(checkmarkRect);
            if (rectData.AnchorMin == rectData.AnchorMax)
            {
                var scaleX = width > 0 && style.Width > 0 ? width / style.Width : 1f;
                var scaleY = height > 0 && style.Height > 0 ? height / style.Height : 1f;
                checkmarkRect.sizeDelta = new Vector2(checkmarkRect.sizeDelta.x * scaleX, checkmarkRect.sizeDelta.y * scaleY);
                checkmarkRect.anchoredPosition = new Vector2(checkmarkRect.anchoredPosition.x * scaleX, checkmarkRect.anchoredPosition.y * scaleY);
            }
        }
        else
        {
            RowElementBuilder.SetRect(checkmarkObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        var checkmarkImage = RowElementBuilder.AddImage(checkmarkObj, style.SelectedBackgroundSprite, Image.Type.Simple, Color.white);
        checkmarkImage.preserveAspect = true;
        checkmarkImage.raycastTarget = false;

        var typeNameObj = RowElementBuilder.CreateObject("type_name", go.transform);
        RowElementBuilder.SetRect(typeNameObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var label = RowElementBuilder.AddText(typeNameObj, style.Unselected, string.Empty, TextAlignmentOptions.Center);
        label.raycastTarget = false;
        label.enableAutoSizing = true;
        label.fontSizeMin = 8f;
        label.fontSizeMax = style.Unselected.FontSize;

        VanillaComponentApplier.ApplyToControl(go.transform, addDySelect: true, addAudio: true);

        return new TabCarouselSlot(rect, button, canvasGroup, label, backgroundObj, style.Selected.Color, style.Unselected.Color, style.ClickSoundEventId);
    }
}
