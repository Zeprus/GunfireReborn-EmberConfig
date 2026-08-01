namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class SliderRow : SettingRowBase
{
    private readonly uint clickSoundEventId;
    private float lastAudioTime = -1f;

    public SliderRow(Transform transform, uint clickSoundEventId) : base(transform)
    {
        this.clickSoundEventId = clickSoundEventId;
    }

    private Slider? slider;
    private TextMeshProUGUI? titleText;
    private TMP_InputField? valueInput;
    private bool isInt;
    private Action<float>? onSliderChanged;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        slider = Transform.Find("Item/Slider_PCunit/Slider")?.GetComponent<Slider>();
        valueInput = Transform.Find("Item/Slider_PCunit/Num")?.GetComponent<TMP_InputField>();
        isInt = entry.Config.SettingType == typeof(int);

        if (slider is null) return;
        slider.onValueChanged ??= new Slider.SliderEvent();

        if (AcceptableValueResolver.TryGetRange(entry.Config.Description?.AcceptableValues, out var min, out var max))
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = isInt;
        }
        else
        {
            slider.minValue = 0f;
            slider.maxValue = isInt ? 100f : 1f;
            slider.wholeNumbers = isInt;
        }

        onSliderChanged = OnSliderChanged;
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(onSliderChanged);

        var trigger = slider.gameObject.GetComponent<EventTrigger>() ?? slider.gameObject.AddComponent<EventTrigger>();
        for (int i = trigger.triggers.Count - 1; i >= 0; i--)
        {
            if (trigger.triggers[i].eventID == EventTriggerType.PointerUp)
                trigger.triggers.RemoveAt(i);
        }

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        Action<BaseEventData> onPointerUp = _ =>
        {
            var value = isInt ? (object)Convert.ToInt32(slider.value) : slider.value;
            SetValue(value, save: true);
        };
        pointerUp.callback.AddListener(onPointerUp);
        trigger.triggers.Add(pointerUp);

        if (valueInput is not null)
        {
            valueInput.onEndEdit ??= new TMP_InputField.SubmitEvent();
            valueInput.onSelect ??= new TMP_InputField.SelectionEvent();

            valueInput.contentType = isInt
                ? TMP_InputField.ContentType.IntegerNumber
                : TMP_InputField.ContentType.DecimalNumber;

            Action<string> onEndEdit = OnEndEdit;
            Action<string> onSelect = OnSelect;

            valueInput.onEndEdit.RemoveAllListeners();
            valueInput.onEndEdit.AddListener(onEndEdit);

            valueInput.onSelect.RemoveAllListeners();
            valueInput.onSelect.AddListener(onSelect);
        }
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (slider is not null && slider.onValueChanged is not null && Entry?.Config.BoxedValue is not null)
        {
            slider.onValueChanged.RemoveAllListeners();
            try
            {
                slider.value = Convert.ToSingle(Entry.Config.BoxedValue);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"EmberConfig: failed to set slider value for '{Entry.Label}': {ex.Message}");
            }
            slider.onValueChanged.AddListener(onSliderChanged!);
        }

        SetValueText();
    }

    private void OnSliderChanged(float value)
    {
        var now = Time.unscaledTime;
        if (now - lastAudioTime > 0.05f)
        {
            lastAudioTime = now;
            WwiseAudio.PostIfValid(clickSoundEventId, slider?.gameObject ?? GameObject);
        }

        if (Entry is null) return;
        var newValue = isInt ? (object)Convert.ToInt32(value) : value;
        SetValue(newValue, save: false);
    }

    private void OnSelect(string _)
    {
        if (valueInput is null) return;
        valueInput.text = FormatValue();
    }

    private void OnEndEdit(string text)
    {
        if (valueInput is null || slider is null) return;

        if (!NumberParser.TryParseFloat(text, out var parsed))
        {
            valueInput.text = FormatValue();
            return;
        }

        var clampedValue = System.Math.Clamp(parsed, slider.minValue, slider.maxValue);
        var clamped = isInt
            ? (object)Convert.ToInt32(clampedValue)
            : (object)clampedValue;

        SetValue(clamped, save: true);
        slider.value = clampedValue;
        valueInput.text = FormatValue();
    }

    private void SetValueText()
    {
        if (valueInput is null || valueInput.isFocused)
            return;

        valueInput.text = FormatValue();
    }

    private string FormatValue()
    {
        var value = Entry?.Config.BoxedValue;
        if (value is null)
            return string.Empty;

        try
        {
            return isInt ? value.ToString()! : $"{Convert.ToSingle(value):F2}";
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
