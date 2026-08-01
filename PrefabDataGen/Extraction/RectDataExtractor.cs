namespace EmberConfig.PrefabDataGen.Extraction;

using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;

internal static class RectDataExtractor
{
    internal static RectData Extract(ComponentNode? rectTransform)
    {
        if (rectTransform is null)
            return Default();

        var anchorMin = rectTransform.GetVector2("m_AnchorMin") ?? new Vector2(0f, 0f);
        var anchorMax = rectTransform.GetVector2("m_AnchorMax") ?? new Vector2(0f, 0f);
        var anchoredPosition = rectTransform.GetVector2("m_AnchoredPosition") ?? new Vector2(0f, 0f);
        var sizeDelta = rectTransform.GetVector2("m_SizeDelta") ?? new Vector2(0f, 0f);
        var pivot = rectTransform.GetVector2("m_Pivot") ?? new Vector2(0.5f, 0.5f);

        return new RectData(
            anchorMin.X, anchorMin.Y,
            anchorMax.X, anchorMax.Y,
            sizeDelta.X, sizeDelta.Y,
            anchoredPosition.X, anchoredPosition.Y,
            pivot.X, pivot.Y);
    }

    internal static RectData Default() =>
        new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.5f, 0.5f);
}
