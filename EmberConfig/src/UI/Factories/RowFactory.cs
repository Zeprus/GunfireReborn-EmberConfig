namespace EmberConfig.UI;

using System;
using EmberConfig.Core;
using EmberConfig.Public;
using UnityEngine;

internal sealed class RowFactory
{
    private readonly UIFinder uiFinder;
    private readonly IKeybindRowServices keybindServices;

    public RowFactory(UIFinder uiFinder, IKeybindRowServices keybindServices)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        this.keybindServices = keybindServices ?? throw new ArgumentNullException(nameof(keybindServices));
    }

    public ISettingRow? CreateRow(ISettingEntry entry, Transform content)
    {
        var catalog = uiFinder.Style;
        if (catalog is null)
        {
            Plugin.Logger?.LogWarning($"EmberConfig: UI style not captured; cannot render '{entry.Label}'.");
            return null;
        }

        var rowType = RowTypeResolver.Resolve(entry);
        var name = $"SL_Row_{entry.Id}";
        var fallbackClickSound = catalog.Tab?.ClickSoundEventId ?? 0u;

        return rowType switch
        {
            RowType.Keybind => BuildKeybindRow(name, entry.Label, catalog, content, fallbackClickSound),
            RowType.Slider => BuildSliderRow(name, entry.Label, catalog, content, fallbackClickSound),
            RowType.Switch => BuildSwitchRow(name, entry.Label, catalog, content, fallbackClickSound, entry.SwitchLabels),
            RowType.Dropdown => BuildDropdownRow(name, entry.Label, catalog, content, fallbackClickSound),
            RowType.Carousel => BuildCarouselRow(name, entry.Label, catalog, content, fallbackClickSound),
            RowType.InputField or _ => BuildInputField(name, catalog, content, fallbackClickSound)
        };
    }

    private static uint ResolveClickSound(uint styleId, uint fallback) => styleId == 0u ? fallback : styleId;

    private ISettingRow? BuildKeybindRow(string name, string label, StyleCatalog catalog, Transform content, uint fallbackClickSound)
    {
        if (catalog.KeybindButton is KeybindButtonStyle keybindStyle)
        {
            var resolvedStyle = keybindStyle with { ClickSoundEventId = ResolveClickSound(keybindStyle.ClickSoundEventId, fallbackClickSound) };
            var transform = KeybindElementBuilder.Build(name, catalog.Row, resolvedStyle, content);
            return new KeybindRow(transform, resolvedStyle, keybindServices);
        }

        LogMissingStyle(RowType.Keybind, label, nameof(catalog.KeybindButton));
        return null;
    }

    private static ISettingRow? BuildSliderRow(string name, string label, StyleCatalog catalog, Transform content, uint fallbackClickSound)
    {
        if (catalog.Slider is SliderStyle sliderStyle)
        {
            var transform = SliderElementBuilder.Build(name, catalog.Row, sliderStyle, content);
            return new SliderRow(transform, ResolveClickSound(sliderStyle.ClickSoundEventId, fallbackClickSound));
        }

        LogMissingStyle(RowType.Slider, label, nameof(catalog.Slider));
        return null;
    }

    private static ISettingRow? BuildSwitchRow(string name, string label, StyleCatalog catalog, Transform content, uint fallbackClickSound, SwitchLabels? switchLabels)
    {
        if (catalog.Switch is SwitchStyle switchStyle)
        {
            var effectiveStyle = switchLabels is not null
                ? switchStyle with { Option1Label = switchLabels.On, Option2Label = switchLabels.Off }
                : switchStyle;
            var transform = SwitchElementBuilder.Build(name, catalog.Row, effectiveStyle, content);
            return new SwitchRow(transform, ResolveClickSound(switchStyle.ClickSoundEventId, fallbackClickSound));
        }

        LogMissingStyle(RowType.Switch, label, nameof(catalog.Switch));
        return null;
    }

    private static ISettingRow? BuildDropdownRow(string name, string label, StyleCatalog catalog, Transform content, uint fallbackClickSound)
    {
        if (catalog.Dropdown is DropdownStyle dropdownStyle)
        {
            var transform = DropdownElementBuilder.Build(name, catalog.Row, dropdownStyle, content);
            return new DropdownRow(transform, ResolveClickSound(dropdownStyle.ClickSoundEventId, fallbackClickSound));
        }

        LogMissingStyle(RowType.Dropdown, label, nameof(catalog.Dropdown));
        return null;
    }

    private static ISettingRow? BuildCarouselRow(string name, string label, StyleCatalog catalog, Transform content, uint fallbackClickSound)
    {
        if (catalog.Carousel is CarouselStyle carouselStyle)
        {
            var transform = CarouselElementBuilder.Build(name, catalog.Row, carouselStyle, content);
            return new CarouselRow(transform, carouselStyle, ResolveClickSound(carouselStyle.ClickSoundEventId, fallbackClickSound));
        }

        LogMissingStyle(RowType.Carousel, label, nameof(catalog.Carousel));
        return null;
    }

    private static ISettingRow BuildInputField(string name, StyleCatalog catalog, Transform content, uint fallbackClickSound)
    {
        var inputTransform = InputElementBuilder.Build(name, catalog.Row, catalog.Input, content);
        var clickSound = ResolveClickSound(catalog.Input?.ClickSoundEventId ?? 0u, fallbackClickSound);
        return new InputFieldRow(inputTransform, clickSound);
    }

    private static void LogMissingStyle(RowType rowType, string label, string styleName)
    {
        Plugin.Logger?.LogWarning($"EmberConfig: cannot render '{label}' as {rowType}; {styleName} style not captured.");
    }

}
