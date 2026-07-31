namespace EmberConfig.PrefabDataGen.Models;

internal sealed record RowRawStyle(
    ExtractedTextAppearance TitleText,
    string? BackgroundSpriteName,
    Color BackgroundColor,
    Color HighlightColor,
    int BackgroundType,
    float Height,
    float Width,
    float TitleWidth,
    float ItemWidth,
    RectData RowRect,
    RectData TitleRect,
    RectData ItemRect);
