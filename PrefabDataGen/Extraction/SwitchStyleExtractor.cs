namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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
            PaddingExtractor.GetPadding(layout, "m_Left"),
            PaddingExtractor.GetPadding(layout, "m_Right"),
            PaddingExtractor.GetPadding(layout, "m_Top"),
            PaddingExtractor.GetPadding(layout, "m_Bottom"));

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
            RectDataExtractor.Extract(option.RectTransform),
            RectDataExtractor.Extract(bgGo.RectTransform),
            RectDataExtractor.Extract(checkGo.RectTransform),
            RectDataExtractor.Extract(labelGo.RectTransform),
            SpriteNameResolver.Resolve(bgImage, assetNameResolver),
            SpriteNameResolver.Resolve(checkImage, assetNameResolver),
            bgImage?.GetColor("m_Color") ?? new Color(0.067f, 0.067f, 0.067f, 1f),
            checkImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 0.102f),
            bgImage?.GetInt("m_Type") ?? 1,
            checkImage?.GetInt("m_Type") ?? 0,
            TextAppearanceExtractor.Extract(labelTextMesh, assetNameResolver),
            labelTextMesh?.GetInt("m_textAlignment") ?? 514,
            ColorBlockExtractor.Extract(toggle) ?? new ExtractedColorBlock(
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
            RectDataExtractor.Extract(clickGroup.RectTransform),
            clickGroupLayout,
            allowSwitchOff,
            0u);
    }

    private static GameObjectNode? FindClickGroup(PrefabDocument document) =>
        document.GameObjects.Values.FirstOrDefault(g =>
            g.Name == "ClickGroup" &&
            g.Components.Any(IsToggleGroup) &&
            g.Components.Any(IsHorizontalLayoutGroup));
}
