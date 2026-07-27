namespace SettingsLib.UI;

using UnityEngine;

internal sealed class StyleCatalog
{
    public RowStyle Row { get; }
    public TabStyle? Tab { get; }
    public GroupHeaderStyle GroupHeader { get; }
    public KeybindButtonStyle? KeybindButton { get; }
    public SliderStyle? Slider { get; }
    public ToggleStyle? Toggle { get; }
    public DropdownStyle? Dropdown { get; }
    public InputStyle? Input { get; }

    private StyleCatalog(
        RowStyle row,
        TabStyle? tab,
        GroupHeaderStyle group,
        KeybindButtonStyle? keybind,
        SliderStyle? slider,
        ToggleStyle? toggle,
        DropdownStyle? dropdown,
        InputStyle? input)
    {
        Row = row;
        Tab = tab;
        GroupHeader = group;
        KeybindButton = keybind;
        Slider = slider;
        Toggle = toggle;
        Dropdown = dropdown;
        Input = input;
    }

    internal static StyleCatalog? CaptureFrom(Transform panelRoot)
    {
        var row = RowStyle.Capture(panelRoot);
        if (row is null)
            return null;

        var rowStyle = row.Value;

        return new StyleCatalog(
            rowStyle,
            TabStyle.Capture(panelRoot, rowStyle.Title),
            GroupHeaderStyle.Capture(panelRoot, rowStyle.Title) ?? GroupHeaderStyle.Default(rowStyle.Title),
            KeybindButtonStyle.Capture(panelRoot, rowStyle.Title, rowStyle.BackgroundSprite),
            SliderStyle.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            ToggleStyle.Capture(panelRoot, rowStyle.BackgroundSprite),
            DropdownStyle.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            InputStyle.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title));
    }
}
