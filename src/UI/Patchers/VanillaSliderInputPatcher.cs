namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds a <see cref="TMP_InputField"/> to the value text of vanilla SettingSlider
/// rows so they can be edited by clicking the number. Failures are caught per-row;
/// the patcher never aborts the whole settings menu.
/// </summary>
internal static class VanillaSliderInputPatcher
{
    internal static void Patch(UIFinder? uiFinder)
    {
        if (uiFinder is null || !uiFinder.IsReady || uiFinder.Viewport is null)
            return;

        var style = uiFinder.Style;
        foreach (var content in uiFinder.GetAllContentPanels())
        {
            if (content is null)
                continue;

            for (int i = 0; i < content.childCount; i++)
            {
                var row = content.GetChild(i);
                if (row is null)
                    continue;

                var sliderPcUnit = row.Find("Slider_PCunit");
                if (sliderPcUnit is null)
                    continue;

                var numObj = sliderPcUnit.Find("Num")?.gameObject;
                var slider = sliderPcUnit.Find("Slider")?.GetComponent<Slider>();
                if (numObj is null || slider is null)
                    continue;

                if (numObj.GetComponent<TMP_InputField>() is not null)
                    continue;

                try
                {
                    AttachInputField(numObj, slider, style?.Row);
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogWarning($"EmberConfig: failed to patch vanilla slider input for '{row.name}': {ex.Message}");
                }
            }
        }
    }

    private static void AttachInputField(GameObject numObj, Slider slider, RowStyle? rowStyle)
    {
        if (numObj.GetComponent<CanvasRenderer>() is null)
            numObj.AddComponent<CanvasRenderer>();

        var image = numObj.GetComponent<Image>() ?? numObj.AddComponent<Image>();
        image.sprite = rowStyle?.BackgroundSprite;
        image.type = rowStyle?.BackgroundType ?? Image.Type.Simple;
        image.color = Color.clear;
        image.raycastTarget = true;

        if (numObj.GetComponent<RectMask2D>() is null)
            numObj.AddComponent<RectMask2D>();

        var text = numObj.GetComponent<TextMeshProUGUI>();
        if (text is not null)
            text.raycastTarget = false;

        var input = numObj.AddComponent<TMP_InputField>();
        input.onEndEdit ??= new TMP_InputField.SubmitEvent();
        input.onSelect ??= new TMP_InputField.SelectionEvent();
        input.textViewport = numObj.GetComponent<RectTransform>();
        input.textComponent = text;
        input.placeholder = null;
        input.targetGraphic = image;
        input.pointSize = text?.fontSize ?? 20f;
        input.selectionColor = new Color(0.25f, 0.25f, 0.25f, 0.75f);
        input.caretColor = text?.color ?? Color.white;
        input.contentType = slider.wholeNumbers
            ? TMP_InputField.ContentType.IntegerNumber
            : TMP_InputField.ContentType.DecimalNumber;
        input.interactable = true;
        input.transition = Selectable.Transition.None;

        Action<string> onSelect = _ => SyncText(input, text);
        Action<string> onEndEdit = t => OnEndEdit(t, input, slider, text);
        input.onSelect.AddListener(onSelect);
        input.onEndEdit.AddListener(onEndEdit);
    }

    private static void SyncText(TMP_InputField input, TextMeshProUGUI? text)
    {
        if (text is not null)
            input.text = text.text;
    }

    private static void OnEndEdit(string text, TMP_InputField input, Slider slider, TextMeshProUGUI? display)
    {
        if (!float.TryParse(text, out var parsed))
        {
            SyncText(input, display);
            return;
        }

        var clamped = System.Math.Clamp(parsed, slider.minValue, slider.maxValue);
        if (slider.wholeNumbers)
            clamped = Convert.ToInt32(clamped);

        slider.value = clamped;
        SyncText(input, display);
    }
}
