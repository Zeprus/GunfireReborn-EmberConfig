namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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

        var text = textArea.FindChild("Text")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find Text TextMeshPro.");

        var placeholder = textArea.FindChild("Placeholder")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find Placeholder TextMeshPro.");

        return new InputRawStyle(
            image.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 0.1f),
            SpriteNameResolver.Resolve(image, assetNameResolver),
            image.GetInt("m_Type") ?? 1,
            TextAppearanceExtractor.Extract(text, assetNameResolver),
            TextAppearanceExtractor.Extract(placeholder, assetNameResolver),
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
}
