namespace EmberConfig.PrefabDataGen.Models;

internal sealed record ExtractedTextAppearance(
    string? FontAssetName,
    string? MaterialName,
    float FontSize,
    Color Color,
    int Alignment,
    int FontStyle,
    float OutlineWidth,
    bool EnableWordWrapping,
    bool EnableAutoSizing,
    int OverflowMode,
    float FontSizeMin,
    float FontSizeMax);
