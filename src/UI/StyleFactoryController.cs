namespace EmberConfig.UI;

using EmberConfig.Generated.PrefabData;
using TMPro;
using UnityEngine;

/// <summary>
/// Central entry point for building the <see cref="StyleCatalog"/>.
/// Delegates to a dedicated factory per UI element.
/// </summary>
internal static class StyleFactoryController
{
    internal static StyleCatalog? Create(Transform panelRoot)
    {
        if (panelRoot is null)
            return null;

        var descriptionText = panelRoot.Find("bg_windows/setting_desc/desc")?.GetComponent<TextMeshProUGUI>();
        var rowStyle = RowStyleFactory.Create(descriptionText);

        return new StyleCatalog(
            rowStyle,
            TabStyleFactory.Create(panelRoot, rowStyle.Title),
            GroupHeaderStyleFactory.Create(panelRoot, rowStyle.Title),
            KeybindButtonStyleFactory.Create(panelRoot, rowStyle.Title, rowStyle.BackgroundSprite),
            SliderStyleFactory.Create(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            SwitchStyleFactory.Create(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            DropdownStyleFactory.Create(rowStyle.Title),
            CarouselStyleFactory.Create(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            InputStyleFactory.Create(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title));
    }
}

internal static class TabStyleFactory
{
    internal static TabStyle? Create(Transform panelRoot, TextAppearance fallback) =>
        TabStyleCapture.Capture(panelRoot, fallback);
}

internal static class GroupHeaderStyleFactory
{
    internal static GroupHeaderStyle Create(Transform panelRoot, TextAppearance fallback) =>
        GroupHeaderStyleCapture.Capture(panelRoot, fallback) ?? GroupHeaderStyle.Default(fallback);
}

internal static class KeybindButtonStyleFactory
{
    internal static KeybindButtonStyle? Create(Transform panelRoot, TextAppearance fallback, Sprite? rowSprite) =>
        KeybindButtonStyleCapture.Capture(panelRoot, fallback, rowSprite);
}

internal static class SliderStyleFactory
{
    internal static SliderStyle? Create(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText) =>
        SliderStyleCapture.Capture(panelRoot, fallbackSprite, fallbackText);
}

internal static class SwitchStyleFactory
{
    internal static SwitchStyle? Create(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText) =>
        SwitchStyleCapture.Capture(panelRoot, fallbackSprite, fallbackText);
}

internal static class CarouselStyleFactory
{
    internal static CarouselStyle Create(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText) =>
        CarouselStyleCapture.Capture(panelRoot, fallbackSprite, fallbackText) ?? CarouselStyle.Default(fallbackSprite, fallbackText);
}

internal static class InputStyleFactory
{
    internal static InputStyle? Create(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText) =>
        InputStyleCapture.Capture(panelRoot, fallbackSprite, fallbackText);
}
