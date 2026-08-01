namespace EmberConfig.UI;

using UnityEngine;

/// <summary>
/// Visual style for a single tab button in the settings tab bar.
/// </summary>
internal readonly record struct TabStyle(
    TextAppearance Selected,
    TextAppearance Unselected,
    float Width,
    float Height)
{
    public Sprite? SelectedBackgroundSprite { get; init; }
    public RectData? SelectedBackgroundRect { get; init; }
    public uint ClickSoundEventId { get; init; }

    internal static TabStyle Fallback(TextAppearance title) =>
        new(
            title with { Color = new Color(0.871f, 0.792f, 0.592f, 1f), FontSize = 30f },
            title with { Color = new Color(0.416f, 0.408f, 0.392f, 1f), FontSize = 30f },
            220f,
            60f)
        {
            SelectedBackgroundSprite = null,
        };
}
