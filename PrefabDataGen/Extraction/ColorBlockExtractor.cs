namespace EmberConfig.PrefabDataGen.Extraction;

using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using YamlDotNet.RepresentationModel;

internal static class ColorBlockExtractor
{
    internal static ExtractedColorBlock? Extract(ComponentNode? toggle)
    {
        var colorsNode = toggle?.GetMapping("m_Colors");
        if (colorsNode is null)
            return null;

        var multiplier = toggle?.GetFloat("m_ColorMultiplier") ?? 1f;
        var fade = toggle?.GetFloat("m_FadeDuration") ?? 0.1f;

        var normalNode = colorsNode.TryGetChild("m_NormalColor", out var n) ? n as YamlMappingNode : null;
        var highlightedNode = colorsNode.TryGetChild("m_HighlightedColor", out var h) ? h as YamlMappingNode : null;
        var pressedNode = colorsNode.TryGetChild("m_PressedColor", out var p) ? p as YamlMappingNode : null;
        var disabledNode = colorsNode.TryGetChild("m_DisabledColor", out var d) ? d as YamlMappingNode : null;

        return new ExtractedColorBlock(
            Color.Multiply(YamlParsers.ParseColor(normalNode) ?? new Color(1f, 1f, 1f, 1f), multiplier),
            Color.Multiply(YamlParsers.ParseColor(highlightedNode) ?? new Color(0.96f, 0.96f, 0.96f, 1f), multiplier),
            Color.Multiply(YamlParsers.ParseColor(pressedNode) ?? new Color(0.78f, 0.78f, 0.78f, 1f), multiplier),
            Color.Multiply(YamlParsers.ParseColor(disabledNode) ?? new Color(0.78f, 0.78f, 0.78f, 0.5f), multiplier),
            multiplier,
            fade);
    }
}
