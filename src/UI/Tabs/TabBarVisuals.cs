namespace SettingsLib.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TabBarVisuals
{
    private TabStyle? style;
    private TabButtonCollection? buttons;
    private M1Toggle? lastVisualActive;

    public void Initialize(TabStyle style, TabButtonCollection buttons)
    {
        this.style = style;
        this.buttons = buttons ?? throw new ArgumentNullException(nameof(buttons));
    }

    public void Reset()
    {
        lastVisualActive = null;
    }

    public void Update()
    {
        if (!style.HasValue || buttons is null)
            return;

        var active = buttons.GetActiveToggle();
        if (active != lastVisualActive)
        {
            ApplyVisuals(active);
            lastVisualActive = active;
        }
    }

    public void ApplyVisuals(M1Toggle? active)
    {
        if (!style.HasValue || buttons is null)
            return;

        var selected = style.Value.Selected;
        var unselected = style.Value.Unselected;

        foreach (var button in buttons.Buttons)
        {
            bool isOn = button.Toggle == active;

            if (button.Background is not null && button.Background.activeSelf != isOn)
                button.Background.SetActive(isOn);

            if (button.Label is not null)
                button.Label.color = isOn ? selected.Color : unselected.Color;
        }
    }
}
