namespace EmberConfig.PrefabDataGen.Models;

internal sealed record CarouselRawStyle(
    ExtractedTextAppearance ValueText,
    ExtractedColorBlock ArrowButtonColorBlock,
    int ArrowButtonTransition,
    string? ArrowImageSpriteName,
    Color ArrowImageColor,
    int ArrowImageType,
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
    string? DotSpriteName,
    int DotType,
    DotGroupLayoutRawStyle DotGroupLayout,
    uint ClickSoundEventId);

internal sealed record DotGroupLayoutRawStyle(
    float Spacing,
    int ChildAlignment,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight);
