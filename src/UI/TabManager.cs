namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using SettingsLib.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orchestrates native and custom settings tabs: detecting the vanilla tab bar,
/// creating custom tabs, routing activation requests, and keeping the tab bar
/// scrolled to the active tab.
/// </summary>
public sealed class TabManager
{
    private readonly UIFinder uiFinder;
    private readonly CustomTabRegistry customTabs = new();
    private readonly CustomTabFactory customTabFactory;
    private readonly NativeTabResolver nativeResolver = new();
    private readonly TabActivationController activationController;
    private readonly TabBarController tabBar = new();

    private M1ToggleGroup? m1ToggleGroup;

    public TabManager(UIFinder uiFinder)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        activationController = new TabActivationController(customTabs, nativeResolver, uiFinder, tabBar);
        customTabFactory = new CustomTabFactory(uiFinder, name => activationController.ActivateCustomTab(name));
    }

    public string? CurrentCustomTab => activationController.CurrentCustomTab;

    public Transform? GetContentForTab(string tabName, bool createIfMissing)
    {
        var normalized = Normalize(tabName);

        if (NativeTabResolver.TryGetNativeContentName(normalized, out var nativeContentName))
        {
            var nativeContent = uiFinder.GetContent(nativeContentName);
            if (nativeContent is not null)
                return nativeContent;
        }

        if (customTabs.TryGet(normalized, out var existingTab))
        {
            if (existingTab.Content != null && existingTab.Toggle != null)
                return existingTab.Content;

            customTabs.Unregister(normalized);
        }

        if (!createIfMissing)
            return null;

        if (m1ToggleGroup is null || uiFinder.Viewport is null || uiFinder.Style is null)
        {
            Plugin.Logger?.LogWarning($"SettingsLib: cannot create custom tab '{normalized}' because the settings UI is not ready.");
            return null;
        }

        var newTab = customTabFactory.Create(normalized, m1ToggleGroup, uiFinder.Style.Tab);
        if (newTab is null)
            return null;

        customTabs.Register(normalized, newTab);
        newTab.Content.gameObject.SetActive(false);
        return newTab.Content;
    }

    public bool IsCustomTab(string tabName) =>
        activationController.IsCustomTab(Normalize(tabName));

    public IEnumerable<Transform> GetAllContentPanels() =>
        activationController.GetAllContentPanels();

    public void OnUIReady()
    {
        if (uiFinder.TabSwitch is null)
            return;

        m1ToggleGroup = uiFinder.TabSwitch.GetComponent<M1ToggleGroup>();

        tabBar.Initialize(uiFinder.TabSwitch, uiFinder.TabSwitch.parent);
        nativeResolver.Scan(uiFinder.TabSwitch);
        activationController.ReRegisterCustomToggles(m1ToggleGroup);

        var activeToggle = FindActiveToggleInGroup();
        tabBar.ScrollTo(activeToggle);

        var registry = SettingsRegistry.Current;
        if (registry is not null)
        {
            foreach (var tab in registry.GetTabs())
                GetContentForTab(tab, true);
        }
    }

    public void OnPanelClosed()
    {
        customTabs.DestroyAll(m1ToggleGroup);
        nativeResolver.Clear();
        tabBar.Reset();
        activationController.ClearCurrentCustomTab();
        m1ToggleGroup = null;
    }

    public void ValidateActiveTab()
    {
        if (m1ToggleGroup is null || m1ToggleGroup.m_Toggles is null)
            return;

        var activeToggle = FindActiveToggleInGroup();
        activationController.OnActiveToggleChanged(activeToggle);
    }

    public void FinalizeLayout()
    {
        foreach (var content in GetAllContentPanels())
        {
            var rect = content.GetComponent<RectTransform>();
            if (rect is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        if (uiFinder.TabSwitch is not null)
        {
            var tabSwitchRect = uiFinder.TabSwitch.GetComponent<RectTransform>();
            if (tabSwitchRect is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(tabSwitchRect);

            tabBar.RefreshSize();
            tabBar.ScrollTo(FindActiveToggleInGroup());
        }
    }

    private M1Toggle? FindActiveToggleInGroup()
    {
        if (m1ToggleGroup is null || m1ToggleGroup.m_Toggles is null)
            return null;

        var toggles = m1ToggleGroup.m_Toggles;
        var count = toggles.Count;
        for (int i = 0; i < count; i++)
        {
            var toggle = toggles[i];
            if (toggle is not null && toggle.isOn)
                return toggle;
        }

        return null;
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
