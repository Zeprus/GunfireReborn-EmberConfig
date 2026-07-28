namespace SettingsLib.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

internal static class ToggleStyleCapture
{
    internal static ToggleStyle? Capture(Transform panelRoot, Sprite? fallbackSprite)
    {
        var toggle = FindToggleTransform(panelRoot);
        if (toggle is null)
            return null;

        var bgImage = FindChildImage(toggle, "Background");
        var checkImage = FindChildImage(toggle, "Checkmark");

        var bgColor = bgImage?.color ?? new Color(0.067f, 0.067f, 0.067f, 1f);
        var checkColor = checkImage?.color ?? new Color(1f, 1f, 1f, 0.102f);
        var bgSprite = bgImage?.sprite ?? fallbackSprite;
        var checkSprite = checkImage?.sprite ?? fallbackSprite;

        return new ToggleStyle(bgColor, checkColor, bgSprite, checkSprite)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(toggle),
        };
    }

    private static Transform? FindToggleTransform(Transform root)
    {
        var toggles = root.GetComponentsInChildren<Toggle>(true);
        for (int i = 0; i < toggles.Length; i++)
        {
            var t = toggles[i];
            if (t is UnityEngine.UI.Toggle)
                return t.transform;
        }

        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (string.Equals(t.name, "Toggle", StringComparison.OrdinalIgnoreCase))
                return t;
        }

        return null;
    }

    private static Image? FindChildImage(Transform parent, string name)
    {
        var images = parent.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (string.Equals(images[i].gameObject.name, name, StringComparison.OrdinalIgnoreCase))
                return images[i];
        }

        return null;
    }
}
