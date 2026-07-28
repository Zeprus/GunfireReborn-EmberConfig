namespace SettingsLib.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class InputStyleCapture
{
    internal static InputStyle? Capture(Transform panelRoot, Sprite? fallbackSprite, TextAppearance fallbackText)
    {
        var input = FindInputFieldTransform(panelRoot);
        if (input is null)
            return null;

        var image = input.GetComponent<Image>();
        var inputRect = RectData.From(input.GetComponent<RectTransform>());

        var textArea = input.Find("Text Area")?.GetComponent<RectTransform>();
        var textAreaRectData = textArea is not null ? RectData.From(textArea) : InputStyle.DefaultTextAreaRect;

        var text = textArea is not null ? textArea.Find("Text")?.GetComponent<TextMeshProUGUI>() : null;
        var textAppearance = text is not null ? TextAppearance.From(text) : fallbackText;

        var placeholder = textArea is not null ? textArea.Find("Placeholder")?.GetComponent<TextMeshProUGUI>() : null;
        var placeholderAppearance = placeholder is not null
            ? TextAppearance.From(placeholder)
            : fallbackText with { Color = new Color(1f, 1f, 1f, 0.4f) };

        return new InputStyle(
            image?.color ?? new Color(1f, 1f, 1f, 0.1f),
            image?.sprite ?? fallbackSprite,
            image?.type ?? Image.Type.Sliced,
            inputRect,
            textAreaRectData,
            textAppearance,
            placeholderAppearance)
        {
            ClickSoundEventId = WwiseAudio.GetEventId(input),
        };
    }

    private static Transform? FindInputFieldTransform(Transform root)
    {
        var inputs = root.GetComponentsInChildren<TMP_InputField>(true);
        for (int i = 0; i < inputs.Length; i++)
        {
            var t = inputs[i].transform;
            if (t.GetComponent<Image>() is not null)
                return t;
        }

        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (string.Equals(t.name, "InputField", StringComparison.OrdinalIgnoreCase))
                return t;
        }

        return null;
    }
}
