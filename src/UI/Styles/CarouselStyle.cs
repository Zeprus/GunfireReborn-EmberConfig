namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct CarouselStyle(
    Sprite? ArrowSprite,
    Color ArrowColor,
    Image.Type ArrowType,
    Sprite? DotSprite,
    Image.Type DotType,
    Color SelectedDotColor,
    Color UnselectedDotColor,
    TextAppearance ValueTextAppearance,
    RectData MutiClickGroupRect,
    RectData PreviousButtonRect,
    RectData NextButtonRect,
    RectData ValueTextRect,
    RectData DotGroupRect,
    RectData DotRect)
{
    public uint ClickSoundEventId { get; init; }

    internal static CarouselStyle Default(Sprite? fallbackSprite, TextAppearance fallbackText) =>
        new(
            fallbackSprite,
            new Color(1f, 1f, 1f, 0.5f),
            Image.Type.Simple,
            fallbackSprite,
            Image.Type.Simple,
            new Color(0.584f, 0.518f, 0.341f, 1f),
            new Color(0.4f, 0.4f, 0.4f, 1f),
            fallbackText with { Color = new Color(0.584f, 0.518f, 0.341f, 1f), FontSize = 18f, Alignment = TextAlignmentOptions.Center },
            new RectData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(360f, 40f), Vector2.zero, new Vector2(0.5f, 0.5f)),
            new RectData(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 40f), new Vector2(-160f, 0f), new Vector2(0.5f, 0.5f)),
            new RectData(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(40f, 40f), new Vector2(160f, 0f), new Vector2(0.5f, 0.5f)),
            new RectData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 26f), new Vector2(0f, 6f), new Vector2(0.5f, 0.5f)),
            new RectData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 6f), new Vector2(0f, -16f), new Vector2(0.5f, 0.5f)),
            new RectData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, 10f), Vector2.zero, new Vector2(0.5f, 0.5f)))
        {
            ClickSoundEventId = 0u,
        };
}
