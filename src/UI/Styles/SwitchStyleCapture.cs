namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class SwitchStyleCapture
{
    internal static SwitchStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        return SwitchStyle.Capture(panelRoot, fallbackSprite, fallbackText);
    }
}
