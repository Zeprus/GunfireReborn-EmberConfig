namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class InputStyleExtractor
{
    internal static InputRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var inputField = FindInputField(document)
            ?? throw new InvalidOperationException("Could not find InputField GameObject.");

        var image = inputField.Components.FirstOrDefault(IsImage)
            ?? throw new InvalidOperationException("InputField is missing Image.");

        var textArea = inputField.FindChild("Text Area")
            ?? throw new InvalidOperationException("Could not find Text Area GameObject.");

        var text = textArea.FindChild("Text")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find Text TextMeshPro.");

        var placeholder = textArea.FindChild("Placeholder")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find Placeholder TextMeshPro.");

        return new InputRawStyle(
            image.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 0.1f),
            ResolveSpriteName(image, assetNameResolver),
            image.GetInt("m_Type") ?? 1,
            ExtractTextAppearance(text, assetNameResolver),
            ExtractTextAppearance(placeholder, assetNameResolver),
            0u);
    }

    private static GameObjectNode? FindInputField(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g =>
            g.Name == "Inputaccount" ||
            g.Components.Any(c =>
                c.TypeName == "MonoBehaviour" &&
                c.Properties.Children.ContainsKey(new YamlScalarNode("m_TextViewport")) &&
                c.Properties.Children.ContainsKey(new YamlScalarNode("m_TextComponent")) &&
                c.Properties.Children.ContainsKey(new YamlScalarNode("m_Placeholder"))));

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Color"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static string? ResolveSpriteName(ComponentNode? image, AssetNameResolver assetNameResolver)
    {
        if (image is null)
            return null;

        var spriteRef = image.GetReference("m_Sprite");
        return assetNameResolver.ResolveName(spriteRef?.Guid);
    }

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

    private static RectData ExtractRectData(ComponentNode? rectTransform)
    {
        if (rectTransform is null)
            return new RectData(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.5f, 0.5f);

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
}
