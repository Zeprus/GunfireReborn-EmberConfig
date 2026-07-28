namespace EmberConfig.UI;

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
        var row = RowStyleCapture.Capture(panelRoot);
        if (row is null)
            return null;

        var rowStyle = row.Value;

        return new StyleCatalog(
            rowStyle,
            TabStyleCapture.Capture(panelRoot, rowStyle.Title),
            GroupHeaderStyleCapture.Capture(panelRoot, rowStyle.Title) ?? GroupHeaderStyle.Default(rowStyle.Title),
            KeybindButtonStyleCapture.Capture(panelRoot, rowStyle.Title, rowStyle.BackgroundSprite),
            SliderStyleCapture.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            ToggleStyleCapture.Capture(panelRoot, rowStyle.BackgroundSprite),
            DropdownStyleCapture.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title),
            InputStyleCapture.Capture(panelRoot, rowStyle.BackgroundSprite, rowStyle.Title));
    }
}
