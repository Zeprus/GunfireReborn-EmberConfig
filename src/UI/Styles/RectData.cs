namespace EmberConfig.UI;

using UnityEngine;

internal readonly record struct RectData(
    Vector2 AnchorMin,
    Vector2 AnchorMax,
    Vector2 SizeDelta,
    Vector2 AnchoredPosition,
    Vector2 Pivot)
{
    internal static RectData From(RectTransform rect) => new(
        rect.anchorMin,
        rect.anchorMax,
        rect.sizeDelta,
        rect.anchoredPosition,
        rect.pivot);

    internal void Apply(RectTransform rect)
    {
        rect.anchorMin = AnchorMin;
        rect.anchorMax = AnchorMax;
        rect.sizeDelta = SizeDelta;
        rect.anchoredPosition = AnchoredPosition;
        rect.pivot = Pivot;
    }
}
