namespace EmberConfig.PrefabDataGen.Models;

internal sealed record KeybindButtonRawStyle(
    ExtractedTextAppearance Text,
    ExtractedTextAppearance NoneText,
    string? SpriteAssetName,
    Color BackgroundColor,
    string? BackgroundSpriteName,
    int BackgroundType,
    ExtractedColorBlock ButtonColorBlock,
    int ButtonTransition,
    RectData PrimaryRect,
    RectData SecondaryRect,
    RectData ItemRect,
    KeybindItemLayoutRawStyle ItemLayout,
    uint ClickSoundEventId);

internal sealed record KeybindItemLayoutRawStyle(
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
