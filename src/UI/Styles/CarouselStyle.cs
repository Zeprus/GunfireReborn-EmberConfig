namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct DotGroupLayout(
    float Spacing,
    TextAnchor ChildAlignment,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight);

internal readonly record struct CarouselStyle(
    TextAppearance ValueText,
    ColorBlock ArrowButtonColorBlock,
    Selectable.Transition ArrowButtonTransition,
    Sprite? ArrowImageSprite,
    Color ArrowImageColor,
    Image.Type ArrowImageType,
    RectData ArrowImageRect,
    RectData NextArrowImageRect,
    RectData ItemRect,
    RectData MutiClickGroupRect,
    RectData PreviousButtonRect,
    RectData NextButtonRect,
    RectData SettingInfoRect,
    RectData NowsetionRect,
    RectData DotGroupRect,
    RectData DotRootRect,
    RectData DotChildRect,
    Color DotBackgroundColor,
    Color DotCheckmarkColor,
    Sprite? DotSprite,
    Image.Type DotType,
    DotGroupLayout DotGroupLayout)
{
    public uint ClickSoundEventId { get; init; }

    internal static class Layout
    {
        internal static DotGroupLayout DefaultDotGroupLayout => new(
            5f, TextAnchor.LowerCenter, 10, 10, 0, 0, false, false, false, false);
    }
}
