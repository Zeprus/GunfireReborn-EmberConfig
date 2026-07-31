namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class RowStyleExtractor
{
    internal static RowRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var row = document.FindBestRow();
        if (row is null)
            throw new InvalidOperationException("Could not find a row with 'Title' and 'Item' children.");

        var titleGo = row.FindChild("Title") ?? throw new InvalidOperationException("Row does not contain a 'Title' child.");
        var itemGo = row.FindChild("Item") ?? throw new InvalidOperationException("Row does not contain an 'Item' child.");

        var titleTextMesh = titleGo.Components.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Title child does not have a TextMeshProUGUI component.");

        var rowImage = row.Components.FirstOrDefault(IsImage);
        var rowSelectable = row.Components.FirstOrDefault(HasColorBlock)
            ?? rowImage;

        var titleAppearance = ExtractTextAppearance(titleTextMesh, assetNameResolver);

        var backgroundSpriteName = rowImage is not null ? ResolveSpriteName(rowImage, assetNameResolver) : null;
        var backgroundType = rowImage?.GetInt("m_Type") ?? 1; // Sliced
        var (backgroundColor, highlightColor) = ExtractColors(rowSelectable, rowImage);

        var rowRect = row.RectTransform is not null ? ExtractRectData(row.RectTransform) : DefaultRectData();
        var titleRect = titleGo.RectTransform is not null ? ExtractRectData(titleGo.RectTransform) : DefaultRectData();
        var itemRect = itemGo.RectTransform is not null ? ExtractRectData(itemGo.RectTransform) : DefaultRectData();

        var height = rowRect.SizeDeltaY > 0 ? rowRect.SizeDeltaY : 50f;
        var width = rowRect.SizeDeltaX > 0 ? rowRect.SizeDeltaX : 1000f;
        var titleWidth = titleRect.SizeDeltaX > 0 ? titleRect.SizeDeltaX : 474.8f;
        var itemWidth = itemRect.SizeDeltaX > 0 ? itemRect.SizeDeltaX : 473.3f;

        return new RowRawStyle(
            titleAppearance,
            backgroundSpriteName,
            backgroundColor,
            highlightColor,
            backgroundType,
            height,
            width,
            titleWidth,
            itemWidth,
            rowRect,
            titleRect,
            itemRect);
    }

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type"));

    private static bool HasColorBlock(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Colors"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static ExtractedTextAppearance ExtractTextAppearance(ComponentNode textMesh, AssetNameResolver assetNameResolver)
    {
        var fontRef = textMesh.GetReference("m_fontAsset");
        var materialRef = textMesh.GetReference("m_sharedMaterial");
        var color = textMesh.GetColor("m_fontColor") ?? new Color(1f, 1f, 1f, 1f);

        return new ExtractedTextAppearance(
            assetNameResolver.ResolveName(fontRef?.Guid),
            assetNameResolver.ResolveName(materialRef?.Guid),
            textMesh.GetFloat("m_fontSize") ?? 20f,
            color,
            textMesh.GetInt("m_textAlignment") ?? 1,
            textMesh.GetInt("m_fontStyle") ?? 0,
            textMesh.GetFloat("m_outlineWidth") ?? 0f,
            textMesh.GetBool("m_enableWordWrapping") is true,
            textMesh.GetBool("m_enableAutoSizing") is true,
            textMesh.GetInt("m_overflowMode") ?? 0,
            textMesh.GetFloat("m_fontSizeMin") ?? 0f,
            textMesh.GetFloat("m_fontSizeMax") ?? 0f);
    }

    private static string? ResolveSpriteName(ComponentNode image, AssetNameResolver assetNameResolver)
    {
        var spriteRef = image.GetReference("m_Sprite");
        return assetNameResolver.ResolveName(spriteRef?.Guid);
    }

    private static (Color Background, Color Highlight) ExtractColors(ComponentNode? selectable, ComponentNode? image)
    {
        var imageColor = image?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f);
        var colorsNode = selectable?.GetMapping("m_Colors");
        if (colorsNode is null)
            return (imageColor, new Color(0.22f, 0.22f, 0.22f, 1f));

        var colorMultiplier = selectable?.GetFloat("m_ColorMultiplier") ?? 1f;
        var normalNode = colorsNode.TryGetChild("m_NormalColor", out var n) ? n as YamlMappingNode : null;
        var highlightedNode = colorsNode.TryGetChild("m_HighlightedColor", out var h) ? h as YamlMappingNode : null;
        var normalColor = YamlParsers.ParseColor(normalNode);
        var highlightedColor = YamlParsers.ParseColor(highlightedNode);

        return (
            MultiplyColor(normalColor ?? imageColor, colorMultiplier),
            MultiplyColor(highlightedColor ?? new Color(0.22f, 0.22f, 0.22f, 1f), colorMultiplier));
    }

    private static Color MultiplyColor(Color color, float multiplier) =>
        new(color.R * multiplier, color.G * multiplier, color.B * multiplier, color.A * multiplier);

    private static RectData ExtractRectData(ComponentNode rectTransform)
    {
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

    private static RectData DefaultRectData() =>
        new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.5f, 0.5f);
}
