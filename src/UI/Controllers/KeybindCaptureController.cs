namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Result returned by <see cref="KeybindCaptureController.Tick"/>.
/// </summary>
internal enum CaptureStatus
{
    /// <summary>Capture is still in progress.</summary>
    InProgress,

    /// <summary>A key has been selected and the row should finish capture.</summary>
    Completed
}

/// <summary>
/// Encapsulates the keybind capture state machine. Inputs are polled every
/// frame; the row is notified when a key has been selected.
/// </summary>
internal sealed class KeybindCaptureController
{
    private static readonly KeyCode[] AllKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

    private enum State { None, WaitRelease, WaitPress, ReleaseNextFrame }

    private State state;
    private string promptText = "...";

    /// <summary>
    /// The config entry being rebound, or <c>null</c> when not capturing.
    /// </summary>
    public ConfigEntry<KeyCode>? Target { get; private set; }

    /// <summary>
    /// The button whose text is updated during capture.
    /// </summary>
    public Button? Button { get; private set; }

    /// <summary>
    /// The text to display on the button while capturing.
    /// </summary>
    public string PromptText => promptText;

    /// <summary>
    /// Whether a capture is currently active.
    /// </summary>
    public bool IsCapturing => state != State.None;

    /// <summary>
    /// The key selected by the user, or <see cref="KeyCode.None"/> if cancelled.
    /// </summary>
    public KeyCode CapturedKey { get; private set; }

    /// <summary>
    /// Begins capturing input for the given config entry.
    /// </summary>
    /// <param name="target">The config entry to update.</param>
    /// <param name="button">The button associated with the binding.</param>
    public void StartCapture(ConfigEntry<KeyCode> target, Button button)
    {
        if (state != State.None || target is null) return;

        this.Target = target;
        this.Button = button;
        CapturedKey = KeyCode.None;
        promptText = "...";
        state = State.WaitRelease;
        SettingsPanelState.IsBlockingClose = true;
    }

    /// <summary>
    /// Resets the capture state without committing a key.
    /// </summary>
    public void Reset()
    {
        state = State.None;
        Target = null;
        Button = null;
        CapturedKey = KeyCode.None;
        promptText = "...";
    }

    /// <summary>
    /// Polls input and advances the capture state machine.
    /// </summary>
    /// <returns>The current capture status.</returns>
    public CaptureStatus Tick()
    {
        switch (state)
        {
            case State.WaitRelease:
                if (Input.anyKey) return CaptureStatus.InProgress;
                state = State.WaitPress;
                return CaptureStatus.InProgress;

            case State.WaitPress:
                if (!Input.anyKeyDown) return CaptureStatus.InProgress;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CapturedKey = KeyCode.None;
                    state = State.ReleaseNextFrame;
                    promptText = "...";
                    return CaptureStatus.InProgress;
                }

                foreach (var key in AllKeyCodes)
                {
                    if (key == KeyCode.None || key == KeyCode.Escape) continue;
                    if (!Input.GetKeyDown(key)) continue;

                    CapturedKey = key;
                    state = State.ReleaseNextFrame;
                    promptText = "...";
                    break;
                }

                return CaptureStatus.InProgress;

            case State.ReleaseNextFrame:
                state = State.None;
                return CaptureStatus.Completed;

            default:
                return CaptureStatus.InProgress;
        }
    }
}
