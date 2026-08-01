namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

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
        var primaryText = primary.FindChild("Text_1")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? primary.Components.FirstOrDefault(IsTextMeshPro);
        var secondaryText = secondary.FindChild("Text_2")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? secondary.Components.FirstOrDefault(IsTextMeshPro);

        var backgroundColor = primaryImage?.GetColor("m_Color") ?? new Color(0.067f, 0.067f, 0.067f, 1f);
        var backgroundSprite = SpriteNameResolver.Resolve(primaryImage, assetNameResolver);

        var buttonColorBlock = ColorBlockExtractor.Extract(primaryButton) ?? new ExtractedColorBlock(
            new Color(1f, 1f, 1f, 0.6f),
            new Color(0.96f, 0.96f, 0.96f, 0.82f),
            new Color(0.784f, 0.784f, 0.784f, 0.82f),
            new Color(0.784f, 0.784f, 0.784f, 0.82f),
            1f,
            0.1f);

        var clickSound = ClickSoundExtractor.Extract(primary) ?? 0u;

        return new KeybindButtonRawStyle(
            TextAppearanceExtractor.Extract(primaryText, assetNameResolver),
            TextAppearanceExtractor.Extract(secondaryText, assetNameResolver),
            primaryText?.GetReference("m_spriteAsset") is FileIdReference spriteRef ? assetNameResolver.ResolveName(spriteRef.Guid) : null,
            backgroundColor,
            backgroundSprite,
            primaryImage?.GetInt("m_Type") ?? 1,
            buttonColorBlock,
            primaryButton?.GetInt("m_Transition") ?? 1,
            RectDataExtractor.Extract(primary.RectTransform),
            RectDataExtractor.Extract(secondary.RectTransform),
            RectDataExtractor.Extract(item.RectTransform),
            new KeybindItemLayoutRawStyle(
                hlg.GetFloat("m_Spacing") ?? 0f,
                hlg.GetInt("m_ChildAlignment") ?? 4,
                PaddingExtractor.GetPadding(hlg, "m_Left"),
                PaddingExtractor.GetPadding(hlg, "m_Right"),
                PaddingExtractor.GetPadding(hlg, "m_Top"),
                PaddingExtractor.GetPadding(hlg, "m_Bottom"),
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
}
