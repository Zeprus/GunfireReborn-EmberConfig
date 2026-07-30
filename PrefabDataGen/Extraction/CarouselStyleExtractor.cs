namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Globalization;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class CarouselStyleExtractor
{
    internal static CarouselRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var item = FindItem(document)
            ?? throw new InvalidOperationException("Could not find Item GameObject.");
        var mutiClickGroup = FindMutiClickGroup(document)
            ?? throw new InvalidOperationException("Could not find MutiClickGroup GameObject.");
        var previous = FindPrevious(document)
            ?? throw new InvalidOperationException("Could not find previous GameObject.");
        var next = FindNext(document)
            ?? throw new InvalidOperationException("Could not find next GameObject.");
        var settingInfo = FindSettingInfo(document)
            ?? throw new InvalidOperationException("Could not find setting_info GameObject.");
        var nowsetion = FindNowsetion(document)
            ?? throw new InvalidOperationException("Could not find nowsetion GameObject.");
        var dotGroup = FindDotGroup(document)
            ?? throw new InvalidOperationException("Could not find Toggle_group GameObject.");
        var firstDot = dotGroup.Children.FirstOrDefault()
            ?? throw new InvalidOperationException("Toggle_group has no dot children.");

        var previousImage = previous.FindChild("img");
        var nextImage = next.FindChild("img");
        var previousButton = previous.Components.FirstOrDefault(IsM1Button);
        var nextButton = next.Components.FirstOrDefault(IsM1Button);
        var button = previousButton ?? nextButton;

        var dotBackground = firstDot.FindDescendants("Background").FirstOrDefault();
        var dotCheckmark = firstDot.FindDescendants("Checkmark").FirstOrDefault();

        var hlg = dotGroup.Components.FirstOrDefault(IsHorizontalLayoutGroup)
            ?? throw new InvalidOperationException("Toggle_group is missing HorizontalLayoutGroup.");

        var valueText = nowsetion.FindChild("Text")?.Components.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find value TextMeshPro.");

        var arrowImage = previousImage;
        var nextArrowImage = nextImage;
        var dotSprite = ResolveSpriteName(dotBackground?.Components.FirstOrDefault(IsImage), assetNameResolver);

        var arrowButtonColorBlock = ExtractColorBlock(button) ?? new ExtractedColorBlock(
            new Color(1f, 1f, 1f, 1f),
            new Color(0.96f, 0.96f, 0.96f, 1f),
            new Color(0.784f, 0.784f, 0.784f, 1f),
            new Color(0.784f, 0.784f, 0.784f, 0.5f),
            1f,
            0.1f);

        return new CarouselRawStyle(
            ExtractTextAppearance(valueText, assetNameResolver),
            arrowButtonColorBlock,
            button?.GetInt("m_Transition") ?? 1,
            ResolveSpriteName(arrowImage?.Components.FirstOrDefault(IsImage), assetNameResolver),
            arrowImage?.Components.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            arrowImage?.Components.FirstOrDefault(IsImage)?.GetInt("m_Type") ?? 0,
            ExtractRectData(arrowImage?.RectTransform),
            ExtractRectData(nextArrowImage?.RectTransform),
            ExtractRectData(item.RectTransform),
            ExtractRectData(mutiClickGroup.RectTransform),
            ExtractRectData(previous.RectTransform),
            ExtractRectData(next.RectTransform),
            ExtractRectData(settingInfo.RectTransform),
            ExtractRectData(nowsetion.RectTransform),
            ExtractRectData(dotGroup.RectTransform),
            ExtractRectData(firstDot.RectTransform),
            ExtractRectData(dotBackground?.RectTransform),
            dotBackground?.Components.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(0.4f, 0.4f, 0.4f, 1f),
            dotCheckmark?.Components.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(0.584f, 0.518f, 0.341f, 1f),
            dotSprite,
            dotBackground?.Components.FirstOrDefault(IsImage)?.GetInt("m_Type") ?? 1,
            new DotGroupLayoutRawStyle(
                hlg.GetFloat("m_Spacing") ?? 5f,
                hlg.GetInt("m_ChildAlignment") ?? 8,
                GetPadding(hlg, "m_Left"),
                GetPadding(hlg, "m_Right"),
                GetPadding(hlg, "m_Top"),
                GetPadding(hlg, "m_Bottom"),
                hlg.GetBool("m_ChildControlWidth") ?? true,
                hlg.GetBool("m_ChildControlHeight") ?? false,
                hlg.GetBool("m_ChildForceExpandWidth") ?? true,
                hlg.GetBool("m_ChildForceExpandHeight") ?? false),
            ExtractClickSound(next) ?? ExtractClickSound(previous) ?? 0u);
    }

    private static GameObjectNode? FindItem(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g => g.Name == "Item");

    private static GameObjectNode? FindMutiClickGroup(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g => g.Name == "MutiClickGroup");

    private static GameObjectNode? FindPrevious(PrefabDocument document) =>
        FindMutiClickGroup(document)?.FindChild("previous");

    private static GameObjectNode? FindNext(PrefabDocument document) =>
        FindMutiClickGroup(document)?.FindChild("next");

    private static GameObjectNode? FindSettingInfo(PrefabDocument document) =>
        FindMutiClickGroup(document)?.FindChild("setting_info");

    private static GameObjectNode? FindNowsetion(PrefabDocument document) =>
        FindSettingInfo(document)?.FindChild("nowsetion");

    private static GameObjectNode? FindDotGroup(PrefabDocument document) =>
        FindSettingInfo(document)?.FindChild("Toggle_group");

    private static int GetPadding(ComponentNode hlg, string key)
    {
        var paddingNode = hlg.GetMapping("m_Padding");
        if (paddingNode is null)
            return 0;

        if (!paddingNode.TryGetChild(key, out var node) || node is not YamlScalarNode scalar)
            return 0;

        return int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Color"));

    private static bool IsM1Button(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_TargetGraphic")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_OnClick")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("CanClickOnDisable"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static bool IsHorizontalLayoutGroup(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Spacing")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_ChildAlignment")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_ChildForceExpandWidth"));

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
