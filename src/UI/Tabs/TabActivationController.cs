namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using System.Linq;
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

    public string? CurrentCustomTab { get; private set; }

    private M1Toggle? lastActiveToggle;

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
        var normalized = Normalize(tabName);
        if (!customTabs.TryGet(normalized, out var tab))
        {
            Plugin.Logger?.LogWarning($"ActivateCustomTab: tab {tabName} not found");
            return;
        }

        if (CurrentCustomTab == normalized && tab.Content.gameObject.activeSelf)
            return;

        CurrentCustomTab = normalized;
        SetAllPanelsActive(false);
        tab.Content.gameObject.SetActive(true);

        if (uiFinder.ScrollRect is not null)
            uiFinder.ScrollRect.content = tab.Content.GetComponent<RectTransform>();

        // TabBarController already owns isOn state; fix it up only if needed.
        if (!tab.Toggle.isOn)
            tab.Toggle.SetIsOnWithoutNotify(true);
    }

    public void DeactivateCustomTabs(M1Toggle? activeNative = null, bool scrollToActive = true)
    {
        CurrentCustomTab = null;

        try
        {
            SetAllPanelsActive(false);

            foreach (var tab in customTabs.All)
            {
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
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"DeactivateCustomTabs: exception: {ex}");
        }
    }

    public void OnActiveToggleChanged(M1Toggle? activeToggle, bool scroll = true)
    {
        if (activeToggle == lastActiveToggle)
            return;

        lastActiveToggle = activeToggle;

        if (activeToggle is null)
        {
            if (CurrentCustomTab is not null)
                DeactivateCustomTabs(null, scrollToActive: false);

            if (scroll)
                tabBar.ScrollTo(null);
            return;
        }

        if (customTabs.TryGetName(activeToggle, out var customName))
        {
            if (CurrentCustomTab != customName)
                ActivateCustomTab(customName);
        }
        else
        {
            DeactivateCustomTabs(activeToggle, scrollToActive: false);
        }

        if (scroll)
            tabBar.ScrollTo(activeToggle);
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

    public void ClearCurrentCustomTab()
    {
        CurrentCustomTab = null;
        lastActiveToggle = null;
    }

    private void SetAllPanelsActive(bool active)
    {
        foreach (var content in GetAllContentPanels())
            content.gameObject.SetActive(active);
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
