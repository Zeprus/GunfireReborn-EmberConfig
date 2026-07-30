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
    float FontSizeMax)
{
    public static ExtractedTextAppearance Default() =>
        new(null, null, 20f, new Color(1f, 1f, 1f, 1f), 1, 0, 0f, false, false, 0, 0f, 0f);
}
