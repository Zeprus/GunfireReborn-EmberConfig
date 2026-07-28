namespace EmberConfig.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class DropdownRow : SettingRowBase
{
    private readonly uint clickSoundEventId;

    public DropdownRow(Transform transform, uint clickSoundEventId) : base(transform)
    {
        this.clickSoundEventId = clickSoundEventId;
    }

    private TMP_Dropdown? dropdown;
    private TextMeshProUGUI? titleText;
    private object?[] options = Array.Empty<object?>();
    private Action<int>? onSelectionChanged;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        dropdown = Transform.Find("Item/Dropdown")?.GetComponent<TMP_Dropdown>();

        if (dropdown is not null)
        {
            options = OptionResolver.Resolve(entry);
            var il2cppOptions = new Il2CppSystem.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (var opt in options)
                il2cppOptions.Add(new TMP_Dropdown.OptionData(OptionResolver.GetDisplayName(opt)));
            dropdown.options = il2cppOptions;

            onSelectionChanged = OnSelectionChanged;
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(onSelectionChanged);

            if (dropdown.captionText is null)
            {
                var label = dropdown.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (label is not null)
                    dropdown.captionText = label;
            }

            AttachClickSound(dropdown);
        }
    }

    private void AttachClickSound(TMP_Dropdown dropdown)
    {
        if (clickSoundEventId == 0u)
            return;

        var trigger = dropdown.gameObject.GetComponent<EventTrigger>() ?? dropdown.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        Action<BaseEventData> onClick = _ => WwiseAudio.PostIfValid(clickSoundEventId, dropdown.gameObject);
        entry.callback.AddListener(onClick);
        trigger.triggers.Add(entry);
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (dropdown is null) return;

        var current = Entry?.Config.BoxedValue;
        int index = current is not null ? Array.IndexOf(options, current) : -1;
        if (index < 0 && current is not null)
            index = OptionResolver.FindIndexByDisplayName(options, current);

        if (index >= 0)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.value = index;
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(onSelectionChanged!);
        }
    }

    private void OnSelectionChanged(int index)
    {
        WwiseAudio.PostIfValid(clickSoundEventId, dropdown?.gameObject ?? GameObject);

        if (Entry is null || index < 0 || index >= options.Length) return;
        Entry.Config.BoxedValue = options[index];
    }


}
