namespace EmberConfig.PrefabDataGen.Models;

internal sealed record TabRawStyle(
    ExtractedTextAppearance Selected,
    ExtractedTextAppearance Unselected,
    float Width,
    float Height,
    string? SelectedBackgroundSpriteName,
    RectData SelectedBackgroundRect,
    uint ClickSoundEventId);
