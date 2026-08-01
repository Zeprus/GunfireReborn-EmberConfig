namespace EmberConfig.UI;

using System;
using EmberConfig.Core;
using TMPro;
using UnityEngine;

internal class InputFieldRow : SettingRowBase
{
    private readonly uint clickSoundEventId;

    public InputFieldRow(Transform transform, uint clickSoundEventId) : base(transform)
    {
        this.clickSoundEventId = clickSoundEventId;
    }

    private TMP_InputField? inputField;
    private TextMeshProUGUI? titleText;
    private Action<string>? onEndEdit;

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        inputField = Transform.Find("Item/InputField")?.GetComponent<TMP_InputField>();

        if (inputField is not null)
        {
            onEndEdit = OnEndEdit;
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(onEndEdit);

            Action<string> onSelected = _ => WwiseAudio.PostIfValid(clickSoundEventId, inputField.gameObject);
            inputField.onSelect.RemoveAllListeners();
            inputField.onSelect.AddListener(onSelected);
        }
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);
        if (inputField is not null)
        {
            inputField.text = Entry?.Config.BoxedValue?.ToString() ?? string.Empty;
            inputField.ForceLabelUpdate();
        }
    }

    private void OnEndEdit(string text)
    {
        if (Entry is null || inputField is null) return;

        try
        {
            var type = Entry.Config.SettingType;
            object? converted = NumberParser.ConvertToType(text, type);

            var acceptableValues = Entry.Config.Description?.AcceptableValues;
            if (acceptableValues is not null && !acceptableValues.IsValid(converted))
            {
                inputField.text = Entry.Config.BoxedValue?.ToString() ?? string.Empty;
                return;
            }

            SetValue(converted, save: true);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"EmberConfig: failed to convert input '{text}' for '{Entry.Label}': {ex.Message}");
            inputField.text = Entry.Config.BoxedValue?.ToString() ?? string.Empty;
        }
    }

}
