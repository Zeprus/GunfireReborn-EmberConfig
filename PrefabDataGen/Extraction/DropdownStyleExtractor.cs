namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using YamlDotNet.RepresentationModel;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

internal static class DropdownStyleExtractor
{
    internal static DropdownRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var dropdownItem = FindDropdownItem(document)
            ?? throw new InvalidOperationException("Could not find a dropdown item with a TMP_Dropdown component.");

        var itemImage = dropdownItem.Components.FirstOrDefault(IsImage);
        var labelGo = dropdownItem.FindChild("Label");
        var arrowGo = dropdownItem.FindChild("Arrow");
        var arrowImage = arrowGo?.Components?.FirstOrDefault(IsImage);
        var controllerLink = dropdownItem.Components.FirstOrDefault(IsControllerLinkToggle);

        var itemStyle = new DropdownItemRawStyle(
            SpriteNameResolver.Resolve(itemImage, assetNameResolver),
            itemImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            itemImage?.GetInt("m_Type") ?? 0,
            RectDataExtractor.Extract(dropdownItem.RectTransform),
            RectDataExtractor.Extract(labelGo?.RectTransform),
            TextAppearanceExtractor.Extract(labelGo?.Components?.FirstOrDefault(IsTextMeshPro), assetNameResolver),
            labelGo?.Components?.FirstOrDefault(IsTextMeshPro)?.GetInt("m_textAlignment") ?? 1,
            RectDataExtractor.Extract(arrowGo?.RectTransform),
            SpriteNameResolver.Resolve(arrowImage, assetNameResolver),
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
        var templateHighlightImage = templateHighlightGo?.Components?.FirstOrDefault(IsImage);
        var itemBgGo = listItemGo.FindChild("Item Background");
        var itemBgImage = itemBgGo?.Components?.FirstOrDefault(IsImage);
        var itemCheckGo = listItemGo.FindChild("Item Checkmark");
        var itemCheckImage = itemCheckGo?.Components?.FirstOrDefault(IsImage);
        var itemLabelGo = listItemGo.FindChild("Item Label");

        var dyScrollRect = templateGo.Components.FirstOrDefault(IsDyCtrlDropDownScrollRect);
        var listItemToggle = listItemGo.Components.FirstOrDefault(IsToggle);

        var templateStyle = new DropdownTemplateRawStyle(
            RectDataExtractor.Extract(templateGo.RectTransform),
            SpriteNameResolver.Resolve(templateImage, assetNameResolver),
            templateImage?.GetColor("m_Color") ?? new Color(0f, 0f, 0f, 0.8f),
            templateImage?.GetInt("m_Type") ?? 0,
            RectDataExtractor.Extract(viewportGo.RectTransform),
            RectDataExtractor.Extract(contentGo.RectTransform),
            RectDataExtractor.Extract(listItemGo.RectTransform),
            RectDataExtractor.Extract(templateHighlightGo?.RectTransform),
            SpriteNameResolver.Resolve(templateHighlightImage, assetNameResolver),
            templateHighlightImage?.GetColor("m_Color") ?? new Color(0.877f, 0.277f, 0.277f, 1f),
            templateHighlightImage?.GetInt("m_Type") ?? 1,
            RectDataExtractor.Extract(itemBgGo?.RectTransform),
            SpriteNameResolver.Resolve(itemBgImage, assetNameResolver),
            itemBgImage?.GetColor("m_Color") ?? new Color(0.749f, 0.675f, 0.471f, 0.502f),
            itemBgImage?.GetInt("m_Type") ?? 0,
            RectDataExtractor.Extract(itemCheckGo?.RectTransform),
            SpriteNameResolver.Resolve(itemCheckImage, assetNameResolver),
            itemCheckImage?.GetColor("m_Color") ?? new Color(0.749f, 0.675f, 0.471f, 0f),
            itemCheckImage?.GetInt("m_Type") ?? 0,
            RectDataExtractor.Extract(itemLabelGo?.RectTransform),
            TextAppearanceExtractor.Extract(itemLabelGo?.Components?.FirstOrDefault(IsTextMeshPro), assetNameResolver),
            itemLabelGo?.Components?.FirstOrDefault(IsTextMeshPro)?.GetInt("m_textAlignment") ?? 0,
            dyScrollRect?.GetInt("ctrlBackKey") ?? 0,
            ColorBlockExtractor.Extract(listItemToggle));

        var scrollbarGo = templateGo.FindChild("Scrollbar")
            ?? throw new InvalidOperationException("Template does not contain a Scrollbar child.");
        var scrollbarImage = scrollbarGo.Components.FirstOrDefault(IsImage);
        var slidingAreaGo = scrollbarGo.FindChild("Sliding Area")
            ?? throw new InvalidOperationException("Scrollbar does not contain a Sliding Area child.");
        var handleGo = slidingAreaGo.FindChild("Handle")
            ?? throw new InvalidOperationException("Sliding Area does not contain a Handle child.");
        var handleImage = handleGo.Components.FirstOrDefault(IsImage);

        var scrollbarStyle = new DropdownScrollbarRawStyle(
            RectDataExtractor.Extract(scrollbarGo.RectTransform),
            SpriteNameResolver.Resolve(scrollbarImage, assetNameResolver),
            scrollbarImage?.GetColor("m_Color") ?? new Color(0.584f, 0.518f, 0.341f, 0.2f),
            scrollbarImage?.GetInt("m_Type") ?? 1,
            RectDataExtractor.Extract(slidingAreaGo.RectTransform),
            RectDataExtractor.Extract(handleGo.RectTransform),
            SpriteNameResolver.Resolve(handleImage, assetNameResolver),
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
}
