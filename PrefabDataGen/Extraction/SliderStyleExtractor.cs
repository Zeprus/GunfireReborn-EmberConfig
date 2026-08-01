namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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

        var backgroundImage = background?.Components?.FirstOrDefault(IsImage);
        var bgImage = bg?.Components?.FirstOrDefault(IsImage);
        var fillImage = fill?.Components?.FirstOrDefault(IsImage);
        var handleImage = handle?.Components?.FirstOrDefault(IsImage);
        var numTextMesh = num?.Components?.FirstOrDefault(IsTextMeshPro);

        var bgSpriteName = SpriteNameResolver.Resolve(backgroundImage, assetNameResolver);
        var bgSprite = SpriteNameResolver.Resolve(bgImage, assetNameResolver) ?? bgSpriteName;
        var fillSpriteName = SpriteNameResolver.Resolve(fillImage, assetNameResolver);
        var handleSpriteName = SpriteNameResolver.Resolve(handleImage, assetNameResolver);

        return new SliderRawStyle(
            backgroundImage?.GetColor("m_Color") ?? new Color(0.329f, 0.302f, 0.302f, 0f),
            bgImage?.GetColor("m_Color") ?? new Color(0.329f, 0.302f, 0.302f, 1f),
            fillImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            handleImage?.GetColor("m_Color") ?? new Color(1f, 1f, 1f, 1f),
            bgSpriteName,
            fillSpriteName,
            handleSpriteName,
            fillImage?.GetInt("m_Type") ?? 3,
            fillImage?.GetInt("m_FillMethod") ?? 0,
            RectDataExtractor.Extract(sliderPcUnit.RectTransform),
            RectDataExtractor.Extract(sliderGo.RectTransform),
            RectDataExtractor.Extract(background?.RectTransform),
            RectDataExtractor.Extract(bg?.RectTransform),
            RectDataExtractor.Extract(fillArea?.RectTransform),
            RectDataExtractor.Extract(fill?.RectTransform),
            RectDataExtractor.Extract(handleArea?.RectTransform),
            RectDataExtractor.Extract(handle?.RectTransform),
            RectDataExtractor.Extract(num?.RectTransform),
            TextAppearanceExtractor.Extract(numTextMesh, assetNameResolver),
            hlg.GetFloat("m_Spacing") ?? 0f,
            hlg.GetInt("m_ChildAlignment") ?? 3,
            PaddingExtractor.GetPadding(hlg, "m_Left"),
            PaddingExtractor.GetPadding(hlg, "m_Right"),
            PaddingExtractor.GetPadding(hlg, "m_Top"),
            PaddingExtractor.GetPadding(hlg, "m_Bottom"),
            hlg.GetBool("m_ChildControlWidth") ?? false,
            hlg.GetBool("m_ChildControlHeight") ?? false,
            hlg.GetBool("m_ChildForceExpandWidth") ?? true,
            hlg.GetBool("m_ChildForceExpandHeight") ?? true,
            ColorBlockExtractor.Extract(sliderComponent) ?? new ExtractedColorBlock(
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
}
