namespace SettingsLib.UI;

using System;
using SettingsLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single visible slot in the tab carousel.
/// </summary>
// test marker
internal sealed class TabCarouselSlot
{
    private readonly RectTransform rectTransform;
    private readonly Button button;
    private readonly CanvasGroup canvasGroup;
    private readonly TextMeshProUGUI label;
    private readonly GameObject background;
    private readonly Color selectedColor;
    private readonly Color unselectedColor;
    private readonly uint clickSoundEventId;

    public TabCarouselSlot(
        RectTransform rectTransform,
        Button button,
        CanvasGroup canvasGroup,
        TextMeshProUGUI label,
        GameObject background,
        Color selectedColor,
        Color unselectedColor,
        uint clickSoundEventId)
    {
        this.rectTransform = rectTransform ?? throw new ArgumentNullException(nameof(rectTransform));
        this.button = button ?? throw new ArgumentNullException(nameof(button));
        this.canvasGroup = canvasGroup ?? throw new ArgumentNullException(nameof(canvasGroup));
        this.label = label ?? throw new ArgumentNullException(nameof(label));
        this.background = background ?? throw new ArgumentNullException(nameof(background));
        this.selectedColor = selectedColor;
        this.unselectedColor = unselectedColor;
        this.clickSoundEventId = clickSoundEventId;
    }

    public int ContentIndex { get; private set; }

    public RectTransform RectTransform => rectTransform;

    public Button Button => button;

    public void SetContent(int contentIndex, string text)
    {
        ContentIndex = contentIndex;
        label.text = text ?? string.Empty;
    }

    public void UpdateVisual(float x, float alpha, float scale, bool isSelected, bool interactable)
    {
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        rectTransform.localScale = new Vector3(scale, scale, 1f);
        canvasGroup.alpha = alpha;

        background.SetActive(isSelected);
        label.color = isSelected ? selectedColor : unselectedColor;

        var canInteract = interactable && alpha > 0.01f;
        button.interactable = canInteract;
        canvasGroup.blocksRaycasts = canInteract;
    }

    public void SetOnClick(Action handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        button.onClick.RemoveAllListeners();
        Action clickHandler = () =>
        {
            Plugin.Logger?.LogInfo($"TabCarouselSlot clicked: contentIndex={ContentIndex}");
            WwiseAudio.PostIfValid(clickSoundEventId, button.gameObject);
            handler();
            Plugin.Logger?.LogInfo($"TabCarouselSlot click handler completed: contentIndex={ContentIndex}");
        };
        button.onClick.AddListener(clickHandler);
    }
}
