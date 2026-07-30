namespace EmberConfig.PrefabDataGen.Models;

internal sealed record SwitchRawStyle(
    SwitchOptionRawStyle Option,
    RectData ClickGroupRect,
    SwitchLayoutRawStyle ClickGroupLayout,
    bool AllowSwitchOff,
    uint ClickSoundEventId);

internal sealed record SwitchOptionRawStyle(
    RectData OptionRect,
    RectData BackgroundRect,
    RectData CheckmarkRect,
    RectData LabelRect,
    string? BackgroundSpriteName,
    string? CheckmarkSpriteName,
    Color BackgroundColor,
    Color CheckmarkColor,
    int BackgroundType,
    int CheckmarkType,
    ExtractedTextAppearance LabelText,
    int LabelAlignment,
    ExtractedColorBlock OptionColorBlock,
    int Transition,
    int ToggleTransition);

internal sealed record SwitchLayoutRawStyle(
    float Spacing,
    int ChildAlignment,
    bool ChildControlWidth,
    bool ChildControlHeight,
    bool ChildForceExpandWidth,
    bool ChildForceExpandHeight,
    int PaddingLeft,
    int PaddingRight,
    int PaddingTop,
    int PaddingBottom);
