namespace EmberConfig.UI;

internal sealed class StyleCatalog
{
    public RowStyle Row { get; }
    public TabStyle? Tab { get; }
    public GroupHeaderStyle GroupHeader { get; }
    public KeybindButtonStyle? KeybindButton { get; }
    public SliderStyle? Slider { get; }
    public SwitchStyle? Switch { get; }
    public DropdownStyle? Dropdown { get; }
    public CarouselStyle? Carousel { get; }
    public InputStyle? Input { get; }

    internal StyleCatalog(
        RowStyle row,
        TabStyle? tab,
        GroupHeaderStyle group,
        KeybindButtonStyle? keybind,
        SliderStyle? slider,
        SwitchStyle? @switch,
        DropdownStyle? dropdown,
        CarouselStyle? carousel,
        InputStyle? input)
    {
        Row = row;
        Tab = tab;
        GroupHeader = group;
        KeybindButton = keybind;
        Slider = slider;
        Switch = @switch;
        Dropdown = dropdown;
        Carousel = carousel;
        Input = input;
    }


}
