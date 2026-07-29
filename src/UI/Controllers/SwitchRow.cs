namespace EmberConfig.UI;

using System;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SwitchRow : SettingRowBase
{
    private readonly uint clickSoundEventId;

    public SwitchRow(Transform transform, uint clickSoundEventId) : base(transform)
    {
        this.clickSoundEventId = clickSoundEventId;
    }

    private M1Toggle? option1;
    private M1Toggle? option2;
    private TextMeshProUGUI? titleText;
    private Action<bool>? onOption1Toggled;
    private Action<bool>? onOption2Toggled;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        option1 = Transform.Find("Item/ClickGroup/1")?.GetComponent<M1Toggle>();
        option2 = Transform.Find("Item/ClickGroup/2")?.GetComponent<M1Toggle>();

        if (option1 is not null && option2 is not null)
        {
            option1.onValueChanged ??= new M1Toggle.ToggleEvent();
            option2.onValueChanged ??= new M1Toggle.ToggleEvent();

            onOption1Toggled = isOn => OnOptionToggled(isOn, true);
            onOption2Toggled = isOn => OnOptionToggled(isOn, false);

            option1.onValueChanged.RemoveAllListeners();
            option2.onValueChanged.RemoveAllListeners();
            option1.onValueChanged.AddListener(onOption1Toggled);
            option2.onValueChanged.AddListener(onOption2Toggled);
        }
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (option1 is not null && option2 is not null && option1.onValueChanged is not null && option2.onValueChanged is not null && Entry?.Config.BoxedValue is bool b)
        {
            option1.onValueChanged.RemoveAllListeners();
            option2.onValueChanged.RemoveAllListeners();
            option1.isOn = b;
            option2.isOn = !b;
            option1.onValueChanged.AddListener(onOption1Toggled!);
            option2.onValueChanged.AddListener(onOption2Toggled!);
        }
    }

    private void OnOptionToggled(bool isOn, bool isOption1)
    {
        WwiseAudio.PostIfValid(clickSoundEventId, option1?.gameObject ?? GameObject);

        if (isOn && Entry is not null)
            Entry.Config.BoxedValue = isOption1;
    }
}
