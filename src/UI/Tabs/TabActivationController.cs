namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles switching between native and custom tab content and tab-bar scrolling.
/// </summary>
internal sealed class TabActivationController
{
    private readonly CustomTabRegistry customTabs;
    private readonly NativeTabResolver nativeResolver;
    private readonly UIFinder uiFinder;
    private readonly TabBarController tabBar;

    private bool isActivating;

    public string? CurrentCustomTab { get; private set; }

    public TabActivationController(
        CustomTabRegistry customTabs,
        NativeTabResolver nativeResolver,
        UIFinder uiFinder,
        TabBarController tabBar)
    {
        this.customTabs = customTabs ?? throw new ArgumentNullException(nameof(customTabs));
        this.nativeResolver = nativeResolver ?? throw new ArgumentNullException(nameof(nativeResolver));
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        this.tabBar = tabBar ?? throw new ArgumentNullException(nameof(tabBar));
    }

    public bool IsCustomTab(string tabName)
    {
        var normalized = Normalize(tabName);
        return !NativeTabResolver.IsNativeTab(normalized) && customTabs.TryGet(normalized, out _);
    }

    public void ActivateCustomTab(string tabName)
    {
        if (isActivating)
            return;

        var normalized = Normalize(tabName);
        if (!customTabs.TryGet(normalized, out var tab))
            return;

        if (CurrentCustomTab == normalized && tab.Content.gameObject.activeSelf)
            return;

        isActivating = true;
        try
        {
            CurrentCustomTab = normalized;
            SetAllPanelsActive(false);
            tab.Content.gameObject.SetActive(true);

            if (uiFinder.ScrollRect is not null)
                uiFinder.ScrollRect.content = tab.Content.GetComponent<RectTransform>();

            if (!tab.Toggle.isOn)
                tab.Toggle.isOn = true;

            tabBar.ScrollTo(tab.Toggle);
        }
        finally
        {
            isActivating = false;
        }
    }

    public void DeactivateCustomTabs(M1Toggle? activeNative = null, bool scrollToActive = true)
    {
        CurrentCustomTab = null;

        foreach (var tab in customTabs.All)
        {
            tab.Content.gameObject.SetActive(false);
            if (tab.Toggle.isOn)
                tab.Toggle.SetIsOnWithoutNotify(false);
        }

        M1Toggle? active = activeNative;
        foreach (var toggle in nativeResolver.NativeToggles)
        {
            if (!toggle.isOn)
                continue;

            if (nativeResolver.TryGetContentName(toggle, out var contentName))
            {
                var content = uiFinder.GetContent(contentName);
                if (content is not null)
                {
                    content.gameObject.SetActive(true);
                    if (uiFinder.ScrollRect is not null)
                        uiFinder.ScrollRect.content = content.GetComponent<RectTransform>();
                }
            }

            active = toggle;
        }

        if (scrollToActive)
            tabBar.ScrollTo(active);
    }

    public void OnActiveToggleChanged(M1Toggle? activeToggle)
    {
        if (activeToggle is null)
        {
            if (CurrentCustomTab is not null)
                DeactivateCustomTabs(null, scrollToActive: false);

            tabBar.ScrollTo(null);
            return;
        }

        if (customTabs.TryGetName(activeToggle, out var customName))
        {
            if (CurrentCustomTab != customName)
                ActivateCustomTab(customName);
        }
        else if (CurrentCustomTab is not null)
        {
            DeactivateCustomTabs(activeToggle, scrollToActive: false);
        }

        tabBar.ScrollTo(activeToggle);
    }

    public void ReRegisterCustomToggles(M1ToggleGroup? toggleGroup)
    {
        if (toggleGroup is null)
            return;

        var toggles = toggleGroup.m_Toggles;
        foreach (var tab in customTabs.All)
        {
            bool alreadyRegistered = false;
            var count = toggles.Count;
            for (int i = 0; i < count; i++)
            {
                if (toggles[i] == tab.Toggle)
                {
                    alreadyRegistered = true;
                    break;
                }
            }

            if (!alreadyRegistered)
            {
                tab.Toggle.m_Group = toggleGroup;
                toggleGroup.RegisterToggle(tab.Toggle);
            }
        }
    }

    public IEnumerable<Transform> GetAllContentPanels()
    {
        foreach (var name in NativeTabResolver.GetNativeContentNames())
        {
            var content = uiFinder.GetContent(name);
            if (content is not null)
                yield return content;
        }

        foreach (var tab in customTabs.All)
        {
            if (tab.Content is not null)
                yield return tab.Content;
        }
    }

    public void ClearCurrentCustomTab() => CurrentCustomTab = null;

    private void SetAllPanelsActive(bool active)
    {
        foreach (var content in GetAllContentPanels())
            content.gameObject.SetActive(active);
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
