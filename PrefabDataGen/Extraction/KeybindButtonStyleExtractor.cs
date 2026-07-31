namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Globalization;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class KeybindButtonStyleExtractor
{
    internal static KeybindButtonRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var item = FindItem(document)
            ?? throw new InvalidOperationException("Could not find Item GameObject.");

        var keyChange = FindKeyChange(document)
            ?? throw new InvalidOperationException("Could not find KeyChange GameObject.");

        var hlg = keyChange.Components.FirstOrDefault(IsHorizontalLayoutGroup)
            ?? throw new InvalidOperationException("KeyChange is missing HorizontalLayoutGroup.");

        var primary = keyChange.FindChild("change_button_1")
            ?? throw new InvalidOperationException("KeyChange has no child named change_button_1.");
        var secondary = keyChange.FindChild("change_button_2")
            ?? throw new InvalidOperationException("KeyChange has no child named change_button_2.");

        var primaryImage = primary.Components.FirstOrDefault(IsImage);
        var primaryButton = primary.Components.FirstOrDefault(IsM1Button);
        var primaryText = primary.FindChild("Text_1")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? primary.Components.FirstOrDefault(IsTextMeshPro);
        var secondaryText = secondary.FindChild("Text_2")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? secondary.Components.FirstOrDefault(IsTextMeshPro);

        var backgroundColor = primaryImage?.GetColor("m_Color") ?? new Color(0.067f, 0.067f, 0.067f, 1f);
        var backgroundSprite = ResolveSpriteName(primaryImage, assetNameResolver);

        var buttonColorBlock = ExtractColorBlock(primaryButton) ?? new ExtractedColorBlock(
            new Color(1f, 1f, 1f, 0.6f),
            new Color(0.96f, 0.96f, 0.96f, 0.82f),
            new Color(0.784f, 0.784f, 0.784f, 0.82f),
            new Color(0.784f, 0.784f, 0.784f, 0.82f),
            1f,
            0.1f);

        var clickSound = ExtractClickSound(primary) ?? 0u;

        return new KeybindButtonRawStyle(
            primaryText is not null ? ExtractTextAppearance(primaryText, assetNameResolver) : ExtractedTextAppearance.Default(),
            secondaryText is not null ? ExtractTextAppearance(secondaryText, assetNameResolver) : ExtractedTextAppearance.Default(),
            primaryText?.GetReference("m_spriteAsset") is FileIdReference spriteRef ? assetNameResolver.ResolveName(spriteRef.Guid) : null,
            backgroundColor,
            backgroundSprite,
            primaryImage?.GetInt("m_Type") ?? 1,
            buttonColorBlock,
            primaryButton?.GetInt("m_Transition") ?? 1,
            ExtractRectData(primary.RectTransform),
            ExtractRectData(secondary.RectTransform),
            ExtractRectData(item.RectTransform),
            new KeybindItemLayoutRawStyle(
                hlg.GetFloat("m_Spacing") ?? 0f,
                hlg.GetInt("m_ChildAlignment") ?? 4,
                GetPadding(hlg, "m_Left"),
                GetPadding(hlg, "m_Right"),
                GetPadding(hlg, "m_Top"),
                GetPadding(hlg, "m_Bottom"),
                hlg.GetBool("m_ChildControlWidth") ?? false,
                hlg.GetBool("m_ChildControlHeight") ?? false,
                hlg.GetBool("m_ChildForceExpandWidth") ?? false,
                hlg.GetBool("m_ChildForceExpandHeight") ?? true),
            clickSound);
    }

    private static GameObjectNode? FindItem(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g => g.Name == "Item");

    private static GameObjectNode? FindKeyChange(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g => g.Name == "KeyChange");

    private static int GetPadding(ComponentNode hlg, string key)
    {
        var paddingNode = hlg.GetMapping("m_Padding");
        if (paddingNode is null)
            return 0;

        if (!paddingNode.TryGetChild(key, out var node) || node is not YamlScalarNode scalar)
            return 0;

        return int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static bool IsHorizontalLayoutGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Spacing")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_ChildAlignment"));

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type"));

    private static bool IsM1Button(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_TargetGraphic")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_OnClick")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("ContinueClickTimeInternal"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static bool IsAkTriggerMouseUp(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("triggerList")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("eventIdInternal"));

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

    private static ExtractedColorBlock? ExtractColorBlock(ComponentNode? button)
    {
        var colorsNode = button?.GetMapping("m_Colors");
        if (colorsNode is null)
            return null;

        var multiplier = button?.GetFloat("m_ColorMultiplier") ?? 1f;
        var fade = button?.GetFloat("m_FadeDuration") ?? 0.1f;

        var normalNode = colorsNode.TryGetChild("m_NormalColor", out var n) ? n as YamlMappingNode : null;
        var highlightedNode = colorsNode.TryGetChild("m_HighlightedColor", out var h) ? h as YamlMappingNode : null;
        var pressedNode = colorsNode.TryGetChild("m_PressedColor", out var p) ? p as YamlMappingNode : null;
        var disabledNode = colorsNode.TryGetChild("m_DisabledColor", out var d) ? d as YamlMappingNode : null;

        return new ExtractedColorBlock(
            MultiplyColor(YamlParsers.ParseColor(normalNode) ?? new Color(1f, 1f, 1f, 1f), multiplier),
            MultiplyColor(YamlParsers.ParseColor(highlightedNode) ?? new Color(0.96f, 0.96f, 0.96f, 1f), multiplier),
            MultiplyColor(YamlParsers.ParseColor(pressedNode) ?? new Color(0.78f, 0.78f, 0.78f, 1f), multiplier),
            MultiplyColor(YamlParsers.ParseColor(disabledNode) ?? new Color(0.78f, 0.78f, 0.78f, 0.5f), multiplier),
            multiplier,
            fade);
    }

    private static Color MultiplyColor(Color color, float multiplier) =>
        new(color.R * multiplier, color.G * multiplier, color.B * multiplier, color.A * multiplier);

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
