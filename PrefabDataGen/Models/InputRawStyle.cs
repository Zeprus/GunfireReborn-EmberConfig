namespace EmberConfig.PrefabDataGen.Models;

internal sealed record InputRawStyle(
    Color BackgroundColor,
    string? BackgroundSpriteName,
    int BackgroundType,
    ExtractedTextAppearance Text,
    ExtractedTextAppearance Placeholder,
    uint ClickSoundEventId);
