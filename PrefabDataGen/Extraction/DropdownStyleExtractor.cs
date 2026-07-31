namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;

internal static class DropdownStyleExtractor
{
    internal static DropdownRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var dropdownItem = FindDropdownItem(document)
            ?? throw new InvalidOperationException("Could not find a dropdown item with a TMP_Dropdown component.");

        var itemImage = dropdownItem.Components.FirstOrDefault(IsImage);
        var labelGo = dropdownItem.FindChild("Label");
        var arrowGo = dropdownItem.FindChild("Arrow");
        var arrowImage = arrowGo?.Components.FirstOrDefault(IsImage);
        var controllerLink = dropdownItem.Components.FirstOrDefault(IsControllerLinkToggle);

        var itemStyle = new DropdownItemRawStyle(
            itemImage is not null ? ResolveSpriteName(itemImage, assetNameResolver) : null,
            itemImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            itemImage?.GetInt("m_Type") ?? 0,
            ExtractRectData(dropdownItem.RectTransform),
            labelGo?.RectTransform is not null ? ExtractRectData(labelGo.RectTransform) : DefaultRectData(),
            labelGo is not null ? ExtractTextAppearance(labelGo.Components.FirstOrDefault(IsTextMeshPro)!, assetNameResolver) : ExtractedTextAppearance.Default(),
            labelGo?.Components.FirstOrDefault(IsTextMeshPro)?.GetInt("m_textAlignment") ?? 1,
            arrowGo?.RectTransform is not null ? ExtractRectData(arrowGo.RectTransform) : DefaultRectData(),
            arrowImage is not null ? ResolveSpriteName(arrowImage, assetNameResolver) : null,
            arrowImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            arrowImage?.GetInt("m_Type") ?? 0,
            controllerLink?.GetInt("ControllerKey") ?? 0);

        var templateGo = dropdownItem.FindChild("Template")
            ?? throw new InvalidOperationException("Dropdown item does not contain a Template child.");

        var templateImage = templateGo.Components.FirstOrDefault(IsImage);
        var viewportGo = templateGo.FindChild("Viewport")
            ?? throw new InvalidOperationException("Template does not contain a Viewport child.");
        var contentGo = viewportGo.FindChild("Content")
            ?? throw new InvalidOperationException("Viewport does not contain a Content child.");
        var listItemGo = contentGo.FindChild("Item")
            ?? throw new InvalidOperationException("Content does not contain an Item child.");

        var templateHighlightGo = listItemGo.FindChild("Image");
        var templateHighlightImage = templateHighlightGo?.Components.FirstOrDefault(IsImage);
        var itemBgGo = listItemGo.FindChild("Item Background");
        var itemBgImage = itemBgGo?.Components.FirstOrDefault(IsImage);
        var itemCheckGo = listItemGo.FindChild("Item Checkmark");
        var itemCheckImage = itemCheckGo?.Components.FirstOrDefault(IsImage);
        var itemLabelGo = listItemGo.FindChild("Item Label");

        var dyScrollRect = templateGo.Components.FirstOrDefault(IsDyCtrlDropDownScrollRect);
        var listItemToggle = listItemGo.Components.FirstOrDefault(IsToggle);

        var templateStyle = new DropdownTemplateRawStyle(
            ExtractRectData(templateGo.RectTransform),
            templateImage is not null ? ResolveSpriteName(templateImage, assetNameResolver) : null,
            templateImage?.GetColor("m_Color") ?? new Color(0f, 0f, 0f, 0.8f),
            templateImage?.GetInt("m_Type") ?? 0,
            viewportGo.RectTransform is not null ? ExtractRectData(viewportGo.RectTransform) : DefaultRectData(),
            contentGo.RectTransform is not null ? ExtractRectData(contentGo.RectTransform) : DefaultRectData(),
            listItemGo.RectTransform is not null ? ExtractRectData(listItemGo.RectTransform) : DefaultRectData(),
            templateHighlightGo?.RectTransform is not null ? ExtractRectData(templateHighlightGo.RectTransform) : DefaultRectData(),
            templateHighlightImage is not null ? ResolveSpriteName(templateHighlightImage, assetNameResolver) : null,
            templateHighlightImage?.GetColor("m_Color") ?? new Color(0.877f, 0.277f, 0.277f, 1f),
            templateHighlightImage?.GetInt("m_Type") ?? 1,
            itemBgGo?.RectTransform is not null ? ExtractRectData(itemBgGo.RectTransform) : DefaultRectData(),
            itemBgImage is not null ? ResolveSpriteName(itemBgImage, assetNameResolver) : null,
            itemBgImage?.GetColor("m_Color") ?? new Color(0.749f, 0.675f, 0.471f, 0.502f),
            itemBgImage?.GetInt("m_Type") ?? 0,
            itemCheckGo?.RectTransform is not null ? ExtractRectData(itemCheckGo.RectTransform) : DefaultRectData(),
            itemCheckImage is not null ? ResolveSpriteName(itemCheckImage, assetNameResolver) : null,
            itemCheckImage?.GetColor("m_Color") ?? new Color(0.749f, 0.675f, 0.471f, 0f),
            itemCheckImage?.GetInt("m_Type") ?? 0,
            itemLabelGo?.RectTransform is not null ? ExtractRectData(itemLabelGo.RectTransform) : DefaultRectData(),
            itemLabelGo is not null ? ExtractTextAppearance(itemLabelGo.Components.FirstOrDefault(IsTextMeshPro)!, assetNameResolver) : ExtractedTextAppearance.Default(),
            itemLabelGo?.Components.FirstOrDefault(IsTextMeshPro)?.GetInt("m_textAlignment") ?? 0,
            dyScrollRect?.GetInt("ctrlBackKey") ?? 0,
            ExtractColorBlock(listItemToggle));

        var scrollbarGo = templateGo.FindChild("Scrollbar")
            ?? throw new InvalidOperationException("Template does not contain a Scrollbar child.");
        var scrollbarImage = scrollbarGo.Components.FirstOrDefault(IsImage);
        var slidingAreaGo = scrollbarGo.FindChild("Sliding Area")
            ?? throw new InvalidOperationException("Scrollbar does not contain a Sliding Area child.");
        var handleGo = slidingAreaGo.FindChild("Handle")
            ?? throw new InvalidOperationException("Sliding Area does not contain a Handle child.");
        var handleImage = handleGo.Components.FirstOrDefault(IsImage);

        var scrollbarStyle = new DropdownScrollbarRawStyle(
            ExtractRectData(scrollbarGo.RectTransform),
            scrollbarImage is not null ? ResolveSpriteName(scrollbarImage, assetNameResolver) : null,
            scrollbarImage?.GetColor("m_Color") ?? new Color(0.584f, 0.518f, 0.341f, 0.2f),
            scrollbarImage?.GetInt("m_Type") ?? 1,
            slidingAreaGo.RectTransform is not null ? ExtractRectData(slidingAreaGo.RectTransform) : DefaultRectData(),
            handleGo.RectTransform is not null ? ExtractRectData(handleGo.RectTransform) : DefaultRectData(),
            handleImage is not null ? ResolveSpriteName(handleImage, assetNameResolver) : null,
            handleImage?.GetColor("m_Color") ?? new Color(0.584f, 0.518f, 0.341f, 0.8f),
            handleImage?.GetInt("m_Type") ?? 1);

        return new DropdownRawStyle(itemStyle, templateStyle, scrollbarStyle);
    }

    private static GameObjectNode? FindDropdownItem(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g =>
            g.Components.Any(c =>
                c.TypeName == "MonoBehaviour" &&
                c.Properties.Children.ContainsKey(new YamlScalarNode("m_Template")) &&
                c.Properties.Children.ContainsKey(new YamlScalarNode("m_CaptionText"))));

    private static bool IsImage(ComponentNode component) =>
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Sprite")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_Type"));

    private static bool IsTextMeshPro(ComponentNode component) =>
        component.TypeName == "TextMeshProUGUI" ||
        (component.TypeName == "MonoBehaviour" && component.Properties.Children.ContainsKey(new YamlScalarNode("m_fontAsset")));

    private static bool IsControllerLinkToggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("LinkedDropDown")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("buttontext"));

    private static bool IsDyCtrlDropDownScrollRect(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("dropdown")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("srviewport")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("srcontent"));

    private static bool IsToggle(ComponentNode component) =>
        component.TypeName == "MonoBehaviour" &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("m_IsOn")) &&
        component.Properties.Children.ContainsKey(new YamlScalarNode("toggleTransition"));

    private static ExtractedTextAppearance ExtractTextAppearance(ComponentNode? textMesh, AssetNameResolver assetNameResolver)
    {
        if (textMesh is null)
            return ExtractedTextAppearance.Default();

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

    private static string? ResolveSpriteName(ComponentNode image, AssetNameResolver assetNameResolver)
    {
        var spriteRef = image.GetReference("m_Sprite");
        return assetNameResolver.ResolveName(spriteRef?.Guid);
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
