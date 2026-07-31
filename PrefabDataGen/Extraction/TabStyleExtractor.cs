namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class TabStyleExtractor
{
    private static readonly Color SelectedColor = new(0.871f, 0.792f, 0.592f, 1f);
    private static readonly Color UnselectedColor = new(0.416f, 0.408f, 0.392f, 1f);

    internal static TabRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var tabSwitch = document.GameObjects.Values.FirstOrDefault(g => g.Name == "tab_switch")
            ?? throw new InvalidOperationException("Could not find tab_switch GameObject.");

        var tabs = tabSwitch.Children.Where(c => c.Components.Any(IsM1Toggle)).ToList();
        if (tabs.Count == 0)
            throw new InvalidOperationException("No M1Toggle tabs found under tab_switch.");

        var referenceTab = tabs[0];
        var textMesh = referenceTab.FindChild("type_name")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find type_name TextMeshPro.");

        var background = referenceTab.FindChild("Background")
            ?? throw new InvalidOperationException("Could not find Background GameObject.");

        var checkmark = background.FindChild("Checkmark")
            ?? throw new InvalidOperationException("Could not find Checkmark GameObject.");

        var checkmarkImage = checkmark.Components.FirstOrDefault(IsImage);
        var checkmarkRect = checkmark.RectTransform;

        var text = ExtractTextAppearance(textMesh, assetNameResolver);

        var selectedText = text with { Color = SelectedColor };
        var unselectedText = text with { Color = UnselectedColor };

        var tabRect = referenceTab.RectTransform;
        var sizeDelta = tabRect?.GetVector2("m_SizeDelta") ?? new Vector2(250f, 60f);

        return new TabRawStyle(
            selectedText,
            unselectedText,
            sizeDelta.X,
            sizeDelta.Y,
            ResolveSpriteName(checkmarkImage, assetNameResolver),
            checkmarkRect is not null ? ExtractRectData(checkmarkRect) : new RectData(0.5f, 0.5f, 0.5f, 0.5f, 261f, 95f, 0f, 0f, 0.5f, 0.5f),
            ExtractClickSound(referenceTab) ?? 0u);
    }

    private static bool IsM1Toggle(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_IsOn")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Group"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static bool IsImage(ComponentNode component) =>
        component.TypeName == "UnityEngine.UI.Image" ||
        (component.TypeName == "MonoBehaviour" &&
         component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
         component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type")));

    private static string? ResolveSpriteName(ComponentNode? image, AssetNameResolver assetNameResolver)
    {
        if (image is null)
            return null;

        var spriteRef = image.GetReference("m_Sprite");
        return assetNameResolver.ResolveName(spriteRef?.Guid);
    }

    private static uint? ExtractClickSound(GameObjectNode button)
    {
        var trigger = button.Components.FirstOrDefault(IsAkTriggerMouseUp);
        var eventId = trigger?.GetInt("eventIdInternal");
        if (eventId is null)
            return null;

        return unchecked((uint)eventId.Value);
    }

    private static bool IsAkTriggerMouseUp(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("triggerList")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("eventIdInternal"));

    private static ExtractedTextAppearance ExtractTextAppearance(ComponentNode textMesh, AssetNameResolver assetNameResolver)
    {
        var fontRef = textMesh.GetReference("m_fontAsset");
        var materialRef = textMesh.GetReference("m_sharedMaterial");

        return new ExtractedTextAppearance(
            assetNameResolver.ResolveName(fontRef?.Guid),
            assetNameResolver.ResolveName(materialRef?.Guid),
            textMesh.GetFloat("m_fontSize") ?? 30f,
            textMesh.GetColor("m_fontColor") ?? new Color(1f, 1f, 1f, 1f),
            textMesh.GetInt("m_textAlignment") ?? 2,
            textMesh.GetInt("m_fontStyle") ?? 0,
            textMesh.GetFloat("m_outlineWidth") ?? 0f,
            textMesh.GetBool("m_enableWordWrapping") is true,
            textMesh.GetBool("m_enableAutoSizing") is true,
            textMesh.GetInt("m_overflowMode") ?? 0,
            textMesh.GetFloat("m_fontSizeMin") ?? 0f,
            textMesh.GetFloat("m_fontSizeMax") ?? 0f);
    }

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
}
