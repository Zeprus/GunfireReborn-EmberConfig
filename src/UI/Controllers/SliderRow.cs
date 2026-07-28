namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
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
    private TextMeshProUGUI? valueText;
    private bool isInt;
    private Action<float>? onSliderChanged;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        slider = Transform.Find("Item/Slider_PCunit/Slider")?.GetComponent<Slider>();
        valueText = Transform.Find("Item/Slider_PCunit/Num")?.GetComponent<TextMeshProUGUI>();
        isInt = entry.Config.SettingType == typeof(int);

        if (slider is null) return;

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


    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (slider is not null && Entry?.Config.BoxedValue is not null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.value = Convert.ToSingle(Entry.Config.BoxedValue);
            slider.onValueChanged.AddListener(onSliderChanged!);
        }

        UpdateValueText();
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
        var newValue = isInt ? (object)Mathf.RoundToInt(value) : value;
        Entry.Config.BoxedValue = newValue;
    }

    private void UpdateValueText()
    {
        if (valueText is null) return;
        var value = Entry?.Config.BoxedValue;
        if (value is null) return;

        valueText.text = isInt ? value.ToString() : $"{Convert.ToSingle(value):F2}";
    }


}
