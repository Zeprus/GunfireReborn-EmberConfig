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
    TextAppearance PlaceholderAppearance)
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
}
