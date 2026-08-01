namespace EmberConfig.PrefabDataGen.Extraction;

using System;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;
using static EmberConfig.PrefabDataGen.Extraction.ComponentPredicates;

internal static class TabStyleExtractor
{
    private static readonly Color SelectedColor = new(0.871f, 0.792f, 0.592f, 1f);
    private static readonly Color UnselectedColor = new(0.416f, 0.408f, 0.392f, 1f);

    internal static TabRawStyle Extract(PrefabDocument document, AssetNameResolver assetNameResolver)
    {
        var tabSwitch = document.GameObjects.Values.FirstOrDefault(g => g.Name == "tab_switch")
            ?? throw new InvalidOperationException("Could not find tab_switch GameObject.");

        var tabs = tabSwitch.Children.Where(c => c.Components.Any(IsM1Toggle)).ToList();
        if (tabs.Count == 0)
            throw new InvalidOperationException("No M1Toggle tabs found under tab_switch.");

        var referenceTab = tabs[0];
        var textMesh = referenceTab.FindChild("type_name")?.Components?.FirstOrDefault(IsTextMeshPro)
            ?? throw new InvalidOperationException("Could not find type_name TextMeshPro.");

        var background = referenceTab.FindChild("Background")
            ?? throw new InvalidOperationException("Could not find Background GameObject.");

        var checkmark = background.FindChild("Checkmark")
            ?? throw new InvalidOperationException("Could not find Checkmark GameObject.");

        var checkmarkImage = checkmark.Components.FirstOrDefault(IsImage);
        var checkmarkRect = checkmark.RectTransform;

        var alignment = textMesh.GetInt("m_textAlignment") ?? 2;
        var text = TextAppearanceExtractor.Extract(textMesh, assetNameResolver, 30f) with { Alignment = alignment };

        var selectedText = text with { Color = SelectedColor };
        var unselectedText = text with { Color = UnselectedColor };

        var tabRect = referenceTab.RectTransform;
        var sizeDelta = tabRect?.GetVector2("m_SizeDelta") ?? new Vector2(250f, 60f);

        return new TabRawStyle(
            selectedText,
            unselectedText,
            sizeDelta.X,
            sizeDelta.Y,
            SpriteNameResolver.Resolve(checkmarkImage, assetNameResolver),
            checkmarkRect is not null ? RectDataExtractor.Extract(checkmarkRect) : new RectData(0.5f, 0.5f, 0.5f, 0.5f, 261f, 95f, 0f, 0f, 0.5f, 0.5f),
            ClickSoundExtractor.Extract(referenceTab) ?? 0u);
    }
}
