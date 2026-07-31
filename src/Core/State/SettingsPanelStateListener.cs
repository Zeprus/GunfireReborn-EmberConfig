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
    /// Set by <see cref="PanelTracker"/> to receive OnEnable callbacks without using
    /// IL2CPP-incompatible <c>Action</c> event add/remove methods.
    /// </summary>
    public Action? OnEnabled;

    /// <summary>
    /// Set by <see cref="PanelTracker"/> to receive OnDisable callbacks.
    /// </summary>
    public Action? OnDisabled;

    private void OnEnable()
    {
        OnEnabled?.Invoke();
    }

    private void OnDisable()
    {
        OnDisabled?.Invoke();
    }
}
