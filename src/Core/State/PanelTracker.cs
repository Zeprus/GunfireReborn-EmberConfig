namespace EmberConfig.Core;

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PanelTracker
{
    private int scanCooldown;
    private Transform? panelRoot;
    private SettingsPanelStateListener? listener;

    public bool IsOpen { get; private set; }
    public Transform? PanelRoot => panelRoot;

    public event Action? Opened;
    public event Action? Closed;

    public void Tick()
    {
        if (panelRoot is null)
        {
            if (--scanCooldown > 0) return;
            scanCooldown = 60;
            TryLocatePanel();
            return;
        }

        if (listener is null)
            AttachListener();
    }

    public void Reset()
    {
        DetachListener();
        panelRoot = null;
        listener = null;
        IsOpen = false;
        scanCooldown = 0;
    }

    private void OnPanelEnabled()
    {
        if (IsOpen) return;
        IsOpen = true;
        Opened?.Invoke();
    }

    private void OnPanelDisabled()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Closed?.Invoke();
    }

    private void TryLocatePanel()
    {
        var panel = FindPanelRoot();
        if (panel is not null)
            panelRoot = panel;
    }

    private static Transform? FindPanelRoot()
    {
        var direct = GameObject.Find("PC_Panel_setting")?.transform;
        if (direct is not null)
            return direct;

        return FindInActiveScene("PC_Panel_setting");
    }

    private static Transform? FindInActiveScene(string name)
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
        }

        return null;
    }

    private void AttachListener()
    {
        if (panelRoot is null) return;

        var go = panelRoot.gameObject;
        listener = go.GetComponent<SettingsPanelStateListener>();
        if (listener is null)
            listener = go.AddComponent<SettingsPanelStateListener>();

        listener.OnEnabled = OnPanelEnabled;
        listener.OnDisabled = OnPanelDisabled;

        if (go.activeInHierarchy)
            OnPanelEnabled();
    }

    private void DetachListener()
    {
        if (listener is null || listener.Equals(null)) return;

        listener.OnEnabled = null;
        listener.OnDisabled = null;
    }
}
