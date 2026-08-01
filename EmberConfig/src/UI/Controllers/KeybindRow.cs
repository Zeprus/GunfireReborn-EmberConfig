namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class KeybindRow : SettingRowBase
{
    private readonly KeybindButtonStyle keybindStyle;
    private readonly IKeybindRowServices keybindServices;
    private readonly KeybindCaptureController controller = new();
    private readonly KeybindCoverMask coverMask;

    public KeybindRow(Transform transform, KeybindButtonStyle keybindStyle, IKeybindRowServices keybindServices) : base(transform)
    {
        this.keybindStyle = keybindStyle;
        this.keybindServices = keybindServices;
        this.coverMask = new KeybindCoverMask(keybindStyle);
    }

    private ConfigEntry<KeyCode>? primaryConfig;
    private ConfigEntry<KeyCode>? secondaryConfig;
    private Button? primaryButton;
    private Button? secondaryButton;
    private TextMeshProUGUI? titleText;

    private bool resumeClosePending;

    public override bool IsCapturing => controller.IsCapturing;

    protected override void OnBind(ISettingEntry settingEntry)
    {
        var kb = (KeybindEntry)settingEntry;
        primaryConfig = kb.Primary;
        secondaryConfig = kb.Secondary;

        titleText = FindTitleText();
        primaryButton = Transform.Find("Item/change_button_1")?.GetComponent<Button>();
        secondaryButton = Transform.Find("Item/change_button_2")?.GetComponent<Button>();

        SetupButton(primaryButton, primaryConfig);
        if (secondaryConfig is not null)
            SetupButton(secondaryButton, secondaryConfig);
        else if (secondaryButton is not null)
            secondaryButton.gameObject.SetActive(false);
    }

    private void SetupButton(Button? button, ConfigEntry<KeyCode>? target)
    {
        if (button is null || target is null) return;

        button.onClick.RemoveAllListeners();
        Action handler = () => StartCapture(target, button);
        button.onClick.AddListener(handler);
        button.interactable = true;
    }

    private void StartCapture(ConfigEntry<KeyCode> target, Button button)
    {
        WwiseAudio.PostIfValid(keybindStyle.ClickSoundEventId, button.gameObject);

        if (controller.IsCapturing) return;

        controller.StartCapture(target, button);

        var panelRoot = PanelLocator.FindPanelRoot(Transform);
        if (panelRoot is not null)
            coverMask.Show(panelRoot, Entry?.Label ?? string.Empty);

        UpdateCaptureText(controller.Button, controller.PromptText);
    }

    private void UpdateCaptureText(Button? button, string value)
    {
        if (button is null) return;
        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text is null) return;

        text.spriteAsset = keybindStyle.SpriteAsset;
        TextAppearanceApplier.Apply(text, keybindStyle.Text);
        text.text = value;
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);
        UpdateButtonText(primaryButton, primaryConfig);
        UpdateButtonText(secondaryButton, secondaryConfig);
    }

    private void UpdateButtonText(Button? button, ConfigEntry<KeyCode>? config)
    {
        if (button is null) return;
        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text is null) return;

        bool isBound = config is not null && config.Value != KeyCode.None;
        text.spriteAsset = keybindStyle.SpriteAsset;
        TextAppearanceApplier.Apply(text, isBound ? keybindStyle.Text : keybindStyle.NoneText);

        if (isBound)
        {
            text.text = $"<sprite name=sck{(int)config!.Value}>";
        }
        else
        {
            text.text = "None";
        }

        button.gameObject.SetActive(config is not null);
    }

    public override void Update()
    {
        if (resumeClosePending)
        {
            resumeClosePending = false;
            SettingsPanelState.IsBlockingClose = false;
        }

        if (!controller.IsCapturing) return;

        var status = controller.Tick();
        UpdateCaptureText(controller.Button, controller.PromptText);

        if (status == CaptureStatus.Completed)
        {
            var target = controller.Target;
            if (target is not null)
            {
                target.Value = controller.CapturedKey;
                target.ConfigFile.Save();
                ShowKeybindToast(controller.CapturedKey);
            }

            EndCapture();
        }
    }

    private void EndCapture()
    {
        controller.Reset();
        resumeClosePending = true;
        coverMask.Hide();
        OnRefresh();
    }

    protected override void OnUnbind()
    {
        resumeClosePending = false;
        controller.Reset();
        SettingsPanelState.IsBlockingClose = false;
        coverMask.Hide();
    }

    private void ShowKeybindToast(KeyCode key)
    {
        keybindServices.ShowKeybindToast(Transform, Entry?.Label ?? string.Empty, key);
    }
}
