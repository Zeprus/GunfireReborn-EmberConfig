namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Globalization;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class SliderStyleExtractor
{
    internal static SliderRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var sliderPcUnit = FindSliderPcUnit(document)
            ?? throw new InvalidOperationException("Could not find Slider_PCunit with HorizontalLayoutGroup.");

        var hlg = sliderPcUnit.Components.FirstOrDefault(IsHorizontalLayoutGroup)
            ?? throw new InvalidOperationException("Slider_PCunit is missing HorizontalLayoutGroup.");

        var sliderGo = sliderPcUnit.FindChild("Slider")
            ?? throw new InvalidOperationException("Slider_PCunit has no child named Slider.");
        var sliderComponent = sliderGo.Components.FirstOrDefault(IsSlider)
            ?? throw new InvalidOperationException("Slider has no M1Slider component.");

        var background = sliderGo.FindChild("Background");
        var bg = background?.FindChild("bg");
        var fillArea = sliderGo.FindChild("Fill Area");
        var fill = fillArea?.FindChild("Fill");
        var handleArea = sliderGo.FindChild("Handle Slide Area");
        var handle = handleArea?.FindChild("Handle");
        var num = sliderPcUnit.FindChild("Num");

        var backgroundImage = background?.Components.FirstOrDefault(IsImage);
        var bgImage = bg?.Components.FirstOrDefault(IsImage);
        var fillImage = fill?.Components.FirstOrDefault(IsImage);
        var handleImage = handle?.Components.FirstOrDefault(IsImage);
        var numTextMesh = num?.Components.FirstOrDefault(IsTextMeshPro);

        var bgSpriteName = ResolveSpriteName(backgroundImage, assetNameResolver);
        var bgSprite = ResolveSpriteName(bgImage, assetNameResolver) ?? bgSpriteName;
        var fillSpriteName = ResolveSpriteName(fillImage, assetNameResolver);
        var handleSpriteName = ResolveSpriteName(handleImage, assetNameResolver);

        var backgroundColor = backgroundImage?.GetColor("m_Color") ?? new Color(0.329f, 0.302f, 0.302f, 0f);
        var bgColor = bgImage?.GetColor("m_Color") ?? new Color(0.329f, 0.302f, 0.302f, 1f);
        var fillColor = fillImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f);
        var handleColor = handleImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f);

        return new SliderRawStyle(
            backgroundColor,
            bgColor,
            fillColor,
            handleColor,
            bgSpriteName,
            fillSpriteName,
            handleSpriteName,
            fillImage?.GetInt("m_Type") ?? 3,
            fillImage?.GetInt("m_FillMethod") ?? 0,
            ExtractRectData(sliderPcUnit.RectTransform),
            ExtractRectData(sliderGo.RectTransform),
            ExtractRectData(background?.RectTransform),
            ExtractRectData(bg?.RectTransform),
            ExtractRectData(fillArea?.RectTransform),
            ExtractRectData(fill?.RectTransform),
            ExtractRectData(handleArea?.RectTransform),
            ExtractRectData(handle?.RectTransform),
            ExtractRectData(num?.RectTransform),
            numTextMesh is not null ? ExtractTextAppearance(numTextMesh, assetNameResolver) : ExtractedTextAppearance.Default(),
            hlg.GetFloat("m_Spacing") ?? 0f,
            hlg.GetInt("m_ChildAlignment") ?? 3,
            GetPadding(hlg, "m_Left"),
            GetPadding(hlg, "m_Right"),
            GetPadding(hlg, "m_Top"),
            GetPadding(hlg, "m_Bottom"),
            hlg.GetBool("m_ChildControlWidth") ?? false,
            hlg.GetBool("m_ChildControlHeight") ?? false,
            hlg.GetBool("m_ChildForceExpandWidth") ?? true,
            hlg.GetBool("m_ChildForceExpandHeight") ?? true,
            ExtractColorBlock(sliderComponent) ?? new ExtractedColorBlock(
                new Color(1f, 1f, 1f, 1f),
                new Color(0.96f, 0.96f, 0.96f, 1f),
                new Color(0.784f, 0.784f, 0.784f, 1f),
                new Color(0.784f, 0.784f, 0.784f, 0.5f),
                1f,
                0.1f),
            sliderComponent.GetInt("m_Transition") ?? 1,
            sliderComponent.GetInt("m_Direction") ?? 0,
            sliderComponent.GetBool("m_WholeNumbers") ?? true,
            sliderComponent.GetFloat("m_MinValue") ?? 0f,
            sliderComponent.GetFloat("m_MaxValue") ?? 100f,
            0u);
    }

    private static GameObjectNode? FindSliderPcUnit(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g =>
            g.Name == "Slider_PCunit" &&
            g.Components.Any(IsHorizontalLayoutGroup));

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

    private static bool IsSlider(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_FillRect")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_HandleRect"));

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

    private static ExtractedColorBlock? ExtractColorBlock(ComponentNode? slider)
    {
        var colorsNode = slider?.GetMapping("m_Colors");
        if (colorsNode is null)
            return null;

        var multiplier = slider?.GetFloat("m_ColorMultiplier") ?? 1f;
        var fade = slider?.GetFloat("m_FadeDuration") ?? 0.1f;

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
