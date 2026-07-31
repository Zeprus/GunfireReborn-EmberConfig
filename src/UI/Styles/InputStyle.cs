namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct InputStyle(
    Color BackgroundColor,
    Sprite? BackgroundSprite,
    Image.Type BackgroundType,
    RectData InputRect,
    RectData TextAreaRect,
    TextAppearance TextAppearance,
    TextAppearance PlaceholderAppearance,
    Color SelectionColor)
{
    public uint ClickSoundEventId { get; init; }

    internal static readonly RectData DefaultTextAreaRect = new(
        Vector2.zero, Vector2.one,
        new Vector2(-8f, -4f), new Vector2(8f, 4f),
        new Vector2(0.5f, 0.5f));

    internal static readonly RectData DefaultInputRect = new(
        new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
        new Vector2(0f, RowElementBuilder.Metrics.ControlHeight),
        Vector2.zero, new Vector2(0.5f, 0.5f));

    internal static readonly Color DefaultBackgroundColor = new(1f, 1f, 1f, 0.1f);

    // Measured from the highlighted dropdown item in-game (average ~ RGB 67,59,39 over the
    // black list background, with alpha 0.5). This is the actual hover/selection fill rather
    // than the prefab's gold Toggle highlightedColor.
    internal static readonly Color DefaultSelectionColor = new(0.3725f, 0.3176f, 0.1843f, 0.5019608f);
}
