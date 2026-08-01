namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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

        var valueText = nowsetion.FindChild("Text")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find value TextMeshPro.");

        var arrowImage = previousImage;
        var nextArrowImage = nextImage;
        var dotSprite = SpriteNameResolver.Resolve(dotBackground?.Components?.FirstOrDefault(IsImage), assetNameResolver);

        var arrowButtonColorBlock = ColorBlockExtractor.Extract(button) ?? new ExtractedColorBlock(
            new Color(1f, 1f, 1f, 1f),
            new Color(0.96f, 0.96f, 0.96f, 1f),
            new Color(0.784f, 0.784f, 0.784f, 1f),
            new Color(0.784f, 0.784f, 0.784f, 0.5f),
            1f,
            0.1f);

        return new CarouselRawStyle(
            TextAppearanceExtractor.Extract(valueText, assetNameResolver),
            arrowButtonColorBlock,
            button?.GetInt("m_Transition") ?? 1,
            SpriteNameResolver.Resolve(arrowImage?.Components?.FirstOrDefault(IsImage), assetNameResolver),
            arrowImage?.Components?.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            arrowImage?.Components?.FirstOrDefault(IsImage)?.GetInt("m_Type") ?? 0,
            RectDataExtractor.Extract(arrowImage?.RectTransform),
            RectDataExtractor.Extract(nextArrowImage?.RectTransform),
            RectDataExtractor.Extract(item.RectTransform),
            RectDataExtractor.Extract(mutiClickGroup.RectTransform),
            RectDataExtractor.Extract(previous.RectTransform),
            RectDataExtractor.Extract(next.RectTransform),
            RectDataExtractor.Extract(settingInfo.RectTransform),
            RectDataExtractor.Extract(nowsetion.RectTransform),
            RectDataExtractor.Extract(dotGroup.RectTransform),
            RectDataExtractor.Extract(firstDot.RectTransform),
            RectDataExtractor.Extract(dotBackground?.RectTransform),
            dotBackground?.Components?.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(0.4f, 0.4f, 0.4f, 1f),
            dotCheckmark?.Components?.FirstOrDefault(IsImage)?.GetColor("m_Color") ?? new Color(0.584f, 0.518f, 0.341f, 1f),
            dotSprite,
            dotBackground?.Components?.FirstOrDefault(IsImage)?.GetInt("m_Type") ?? 1,
            new DotGroupLayoutRawStyle(
                hlg.GetFloat("m_Spacing") ?? 5f,
                hlg.GetInt("m_ChildAlignment") ?? 8,
                PaddingExtractor.GetPadding(hlg, "m_Left"),
                PaddingExtractor.GetPadding(hlg, "m_Right"),
                PaddingExtractor.GetPadding(hlg, "m_Top"),
                PaddingExtractor.GetPadding(hlg, "m_Bottom"),
                hlg.GetBool("m_ChildControlWidth") ?? true,
                hlg.GetBool("m_ChildControlHeight") ?? false,
                hlg.GetBool("m_ChildForceExpandWidth") ?? true,
                hlg.GetBool("m_ChildForceExpandHeight") ?? false),
            ClickSoundExtractor.Extract(next) ?? ClickSoundExtractor.Extract(previous) ?? 0u);
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
}
