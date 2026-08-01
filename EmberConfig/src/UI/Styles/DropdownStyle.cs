namespace EmberConfig.UI;

using TMPro;
using UnityEngine;

/// <summary>
/// Visual style for a <see cref="TMP_Dropdown"/> row, split into the collapsed
/// item, the expanded list template, and the scrollbar.
/// </summary>
internal readonly record struct DropdownStyle(
    DropdownItemStyle Item,
    DropdownTemplateStyle Template,
    DropdownScrollbarStyle Scrollbar)
{
    public uint ClickSoundEventId { get; init; }

    internal static DropdownStyle Default(Sprite? fallbackSprite, TextAppearance fallbackText) =>
        new(
            DropdownItemStyle.Default(fallbackSprite, fallbackText),
            DropdownTemplateStyle.Default(fallbackSprite, fallbackText),
            DropdownScrollbarStyle.Default(fallbackSprite));
}
