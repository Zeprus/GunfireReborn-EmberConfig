namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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
        var rowSelectable = row.Components.FirstOrDefault(HasColorBlock) ?? rowImage;

        var titleAppearance = TextAppearanceExtractor.Extract(titleTextMesh, assetNameResolver);

        var backgroundSpriteName = SpriteNameResolver.Resolve(rowImage, assetNameResolver);
        var backgroundType = rowImage?.GetInt("m_Type") ?? 1;
        var (backgroundColor, highlightColor) = ExtractColors(rowSelectable, rowImage);

        var rowRect = RectDataExtractor.Extract(row.RectTransform);
        var titleRect = RectDataExtractor.Extract(titleGo.RectTransform);
        var itemRect = RectDataExtractor.Extract(itemGo.RectTransform);

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

    private static bool HasColorBlock(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Colors"));

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
            Color.Multiply(normalColor ?? imageColor, colorMultiplier),
            Color.Multiply(highlightedColor ?? new Color(0.22f, 0.22f, 0.22f, 1f), colorMultiplier));
    }
}
