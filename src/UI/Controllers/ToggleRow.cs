namespace EmberConfig.UI;

using System;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class ToggleRow : SettingRowBase
{
    private readonly uint clickSoundEventId;

    public ToggleRow(Transform transform, uint clickSoundEventId) : base(transform)
    {
        this.clickSoundEventId = clickSoundEventId;
    }

    private M1Toggle? toggle;
    private TextMeshProUGUI? titleText;
    private Action<bool>? onToggled;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        toggle = Transform.Find("Item/Toggle")?.GetComponent<M1Toggle>();

        if (toggle is not null)
        {
            onToggled = OnToggled;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(onToggled);
        }
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (toggle is not null && Entry?.Config.BoxedValue is bool b)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = b;
            toggle.onValueChanged.AddListener(onToggled!);
        }
    }

    private void OnToggled(bool isOn)
    {
        WwiseAudio.PostIfValid(clickSoundEventId, toggle?.gameObject ?? GameObject);

        if (Entry is not null)
            Entry.Config.BoxedValue = isOn;
    }
}
