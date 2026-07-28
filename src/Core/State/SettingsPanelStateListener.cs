namespace EmberConfig.Core;

using System;
using UnityEngine;

/// <summary>
/// IL2CPP <see cref="MonoBehaviour"/> attached to the vanilla settings panel so
/// <see cref="PanelTracker"/> can react to OnEnable/OnDisable without polling.
/// </summary>
public class SettingsPanelStateListener : MonoBehaviour
{
    public SettingsPanelStateListener(IntPtr ptr) : base(ptr) { }

    /// <summary>
    /// Raised when the panel is enabled (opened).
    /// </summary>
    public event Action? PanelEnabled;

    /// <summary>
    /// Raised when the panel is disabled (closed).
    /// </summary>
    public event Action? PanelDisabled;

    private void OnEnable()
    {
        PanelEnabled?.Invoke();
    }

    private void OnDisable()
    {
        PanelDisabled?.Invoke();
    }
}
