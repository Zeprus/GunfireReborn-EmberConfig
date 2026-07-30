namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Globalization;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class SwitchStyleExtractor
{
    internal static SwitchRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var clickGroup = FindClickGroup(document)
            ?? throw new InvalidOperationException("Could not find ClickGroup with ToggleGroup and HorizontalLayoutGroup.");

        var layout = clickGroup.Components.FirstOrDefault(IsHorizontalLayoutGroup)
            ?? throw new InvalidOperationException("ClickGroup is missing HorizontalLayoutGroup.");
        var toggleGroup = clickGroup.Components.FirstOrDefault(IsToggleGroup)
            ?? throw new InvalidOperationException("ClickGroup is missing ToggleGroup.");

        var allowSwitchOff = toggleGroup.GetBool("m_AllowSwitchOff") ?? false;

        var clickGroupLayout = new SwitchLayoutRawStyle(
            layout.GetFloat("m_Spacing") ?? 0f,
            layout.GetInt("m_ChildAlignment") ?? 4,
            layout.GetBool("m_ChildControlWidth") ?? false,
            layout.GetBool("m_ChildControlHeight") ?? false,
            layout.GetBool("m_ChildForceExpandWidth") ?? false,
            layout.GetBool("m_ChildForceExpandHeight") ?? true,
            GetPadding(layout, "m_Left"),
            GetPadding(layout, "m_Right"),
            GetPadding(layout, "m_Top"),
            GetPadding(layout, "m_Bottom"));

        var option = clickGroup.Children.FirstOrDefault()
            ?? throw new InvalidOperationException("ClickGroup has no option children.");

        var toggle = option.Components.FirstOrDefault(IsToggle)
            ?? throw new InvalidOperationException("Option has no Toggle.");
        var bgGo = option.FindChild("Background")
            ?? throw new InvalidOperationException("Option has no Background.");
        var checkGo = bgGo.FindChild("Checkmark")
            ?? throw new InvalidOperationException("Background has no Checkmark.");
        var labelGo = option.FindChild("Label")
            ?? throw new InvalidOperationException("Option has no Label.");

        var bgImage = bgGo.Components.FirstOrDefault(IsImage);
        var checkImage = checkGo.Components.FirstOrDefault(IsImage);
        var labelTextMesh = labelGo.Components.FirstOrDefault(IsTextMeshPro);

        var optionStyle = new SwitchOptionRawStyle(
            ExtractRectData(option.RectTransform),
            ExtractRectData(bgGo.RectTransform),
            ExtractRectData(checkGo.RectTransform),
            ExtractRectData(labelGo.RectTransform),
            bgImage is not null ? ResolveSpriteName(bgImage, assetNameResolver) : null,
            checkImage is not null ? ResolveSpriteName(checkImage, assetNameResolver) : null,
            bgImage?.GetColor("m_Color") ?? new Color(0.067f, 0.067f, 0.067f, 1f),
            checkImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 0.102f),
            bgImage?.GetInt("m_Type") ?? 1,
            checkImage?.GetInt("m_Type") ?? 0,
            labelTextMesh is not null ? ExtractTextAppearance(labelTextMesh, assetNameResolver) : ExtractedTextAppearance.Default(),
            labelTextMesh?.GetInt("m_textAlignment") ?? 514,
            ExtractColorBlock(toggle) ?? new ExtractedColorBlock(
                new Color(1f, 1f, 1f, 0.6f),
                new Color(0.96f, 0.96f, 0.96f, 0.82f),
                new Color(0.784f, 0.784f, 0.784f, 0.82f),
                new Color(0.784f, 0.784f, 0.784f, 0.82f),
                1f,
                0.1f),
            toggle.GetInt("m_Transition") ?? 1,
            toggle.GetInt("toggleTransition") ?? 1);

        return new SwitchRawStyle(
            optionStyle,
            ExtractRectData(clickGroup.RectTransform),
            clickGroupLayout,
            allowSwitchOff,
            0u);
    }

    private static int GetPadding(ComponentNode layout, string key)
    {
        var paddingNode = layout.GetMapping("m_Padding");
        if (paddingNode is null)
            return 0;

        if (!paddingNode.TryGetChild(key, out var node) || node is not YamlScalarNode scalar)
            return 0;

        return int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static GameObjectNode? FindClickGroup(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g =>
            g.Name == "ClickGroup" &&
            g.Components.Any(IsToggleGroup) &&
            g.Components.Any(IsHorizontalLayoutGroup));

    private static bool IsToggleGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_AllowSwitchOff"));

    private static bool IsHorizontalLayoutGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Spacing")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_ChildAlignment"));

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type"));

    private static bool IsToggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_IsOn")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("toggleTransition"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static string? ResolveSpriteName(ComponentNode image, AssetNameResolver assetNameResolver)
    {
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

    private static ExtractedColorBlock? ExtractColorBlock(ComponentNode? toggle)
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
            return DefaultRectData();

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
