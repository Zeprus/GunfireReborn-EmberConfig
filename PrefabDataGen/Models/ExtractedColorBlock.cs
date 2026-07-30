namespace EmberConfig.PrefabDataGen.Models;

internal sealed record ExtractedColorBlock(
    Color NormalColor,
    Color HighlightedColor,
    Color PressedColor,
    Color DisabledColor,
    float ColorMultiplier,
    float FadeDuration);
