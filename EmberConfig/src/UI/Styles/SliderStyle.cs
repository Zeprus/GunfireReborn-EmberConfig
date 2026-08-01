namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual style for a vanilla numeric slider control.
/// </summary>
internal readonly record struct SliderStyle(
    Color BackgroundColor,
    Color BgColor,
    Color FillColor,
    Color HandleColor,
    Sprite? BackgroundSprite,
    Sprite? FillSprite,
    Sprite? HandleSprite,
    Image.Type FillImageType,
    Image.FillMethod FillFillMethod,
    RectData SliderPcUnitRect,
    RectData SliderRect,
    RectData BackgroundRect,
    RectData BgRect,
    RectData FillAreaRect,
    RectData FillRect,
    RectData HandleSlideAreaRect,
    RectData HandleRect,
    RectData NumRect,
    TextAppearance NumTextAppearance,
    float Spacing,
    TextAnchor ChildAlignment,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight,
    ColorBlock SliderColorBlock,
    Selectable.Transition SliderTransition,
    Slider.Direction Direction,
    bool WholeNumbers,
    float MinValue,
    float MaxValue)
{
    public uint ClickSoundEventId { get; init; }
}
