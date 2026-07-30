namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Layout data for a switch's internal <see cref="HorizontalLayoutGroup"/>.
/// </summary>
internal readonly record struct SwitchLayoutGroup(
    float Spacing,
    TextAnchor ChildAlignment,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom);

/// <summary>
/// Visual style for a vanilla two-option "Switch" boolean control.
/// </summary>
internal readonly record struct SwitchStyle(
    Color OptionBackgroundColor,
    Color OptionCheckmarkColor,
    Sprite? OptionBackgroundSprite,
    Sprite? OptionCheckmarkSprite,
    Image.Type OptionBackgroundType,
    Image.Type OptionCheckmarkType,
    TextAppearance LabelTextAppearance,
    RectData ClickGroupRect,
    RectData OptionRect,
    RectData LabelRect,
    RectData BackgroundRect,
    RectData CheckmarkRect,
    SwitchLayoutGroup ClickGroupLayout,
    ColorBlock OptionColorBlock,
    Selectable.Transition OptionTransition,
    bool AllowSwitchOff,
    uint ClickSoundEventId)
{
    public string Option1Label { get; init; } = "On";
    public string Option2Label { get; init; } = "Off";
}
