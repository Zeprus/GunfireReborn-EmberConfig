namespace EmberConfig.PrefabDataGen.Extraction;

using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;

internal static class TextAppearanceExtractor
{
    internal static ExtractedTextAppearance Extract(ComponentNode? textMesh, AssetNameResolver resolver, float defaultFontSize = 20f)
    {
        if (textMesh is null)
            return ExtractedTextAppearance.Default() with { FontSize = defaultFontSize };

        var fontRef = textMesh.GetReference("m_fontAsset");
        var materialRef = textMesh.GetReference("m_sharedMaterial");
        var color = textMesh.GetColor("m_fontColor") ?? new Color(1f, 1f, 1f, 1f);

        return new ExtractedTextAppearance(
            resolver.ResolveName(fontRef?.Guid),
            resolver.ResolveName(materialRef?.Guid),
            textMesh.GetFloat("m_fontSize") ?? defaultFontSize,
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
}
