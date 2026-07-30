namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

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
    Vector2 OptionSize,
    float Spacing,
    TextAnchor ChildAlignment,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight)
{
    public string Option1Label { get; init; } = "On";
    public string Option2Label { get; init; } = "Off";
    public uint ClickSoundEventId { get; init; }
}
