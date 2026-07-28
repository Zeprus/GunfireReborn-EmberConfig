namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using SettingsLib;
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
    private int activeTabIndex;

    public TabManager(UIFinder uiFinder)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        activationController = new TabActivationController(customTabs, nativeResolver, uiFinder, tabBar);
        customTabFactory = new CustomTabFactory(uiFinder, name => activationController.ActivateCustomTab(name));
        tabBar.OnTabSelected += SetActiveTab;
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
        Plugin.Logger?.LogInfo("TabManager.OnUIReady");
        if (uiFinder.TabSwitch is null || uiFinder.Style is null)
        {
            Plugin.Logger?.LogWarning($"TabManager.OnUIReady skipped: TabSwitch={(uiFinder.TabSwitch != null)}, Style={(uiFinder.Style != null)}");
            return;
        }

        m1ToggleGroup = uiFinder.TabSwitch.GetComponent<M1ToggleGroup>();

        var tabStyle = uiFinder.Style.Tab ?? TabStyle.Fallback(uiFinder.Style.Row.Title);
        Plugin.Logger?.LogInfo($"TabManager.OnUIReady: initializing tabBar with style.Height={tabStyle.Height} style.Width={tabStyle.Width}");
        tabBar.Initialize(uiFinder.TabSwitch, uiFinder.TabSwitch.parent, tabStyle);
        nativeResolver.Scan(uiFinder.TabSwitch);
        activationController.ReRegisterCustomToggles(m1ToggleGroup);

        var registry = SettingsRegistry.Current;
        if (registry is not null)
        {
            foreach (var tab in registry.GetTabs())
                GetContentForTab(tab, true);
        }

        if (m1ToggleGroup is not null)
        {
            Plugin.Logger?.LogInfo("TabManager.OnUIReady: rebuilding tabBar");
            tabBar.Rebuild(m1ToggleGroup);
        }

        var activeToggle = FindActiveToggleInGroup();
        activeTabIndex = FindToggleIndex(activeToggle);
        tabBar.ScrollTo(activeToggle);
        AttachArrowListeners();
    }

    public void OnPanelClosed()
    {
        DetachArrowListeners();
        customTabs.DestroyAll(m1ToggleGroup);
        nativeResolver.Clear();
        tabBar.Reset();
        activationController.ClearCurrentCustomTab();
        m1ToggleGroup = null;
        activeTabIndex = 0;
    }

    public void ValidateActiveTab()
    {
        if (m1ToggleGroup is null)
            return;

        var activeToggle = FindActiveToggleInGroup();
        if (activeToggle is not null)
            activeTabIndex = FindToggleIndex(activeToggle);

        activationController.OnActiveToggleChanged(activeToggle);
    }


    public void Update(float deltaTime)
    {
        tabBar.Update(deltaTime);
    }

    private void SetActiveTab(int index)
    {
        Plugin.Logger?.LogInfo($"TabManager.SetActiveTab: requested={index}, current={activeTabIndex}");

        var count = tabBar.RingCount;
        if (count == 0)
        {
            Plugin.Logger?.LogWarning("TabManager.SetActiveTab: ring count is 0");
            return;
        }

        index = TabCarouselLayout.Mod(index, count);
        if (index == activeTabIndex)
        {
            Plugin.Logger?.LogInfo($"TabManager.SetActiveTab: index {index} already active");
            return;
        }

        var toggle = tabBar.GetToggle(index);
        if (toggle is null)
        {
            Plugin.Logger?.LogWarning($"TabManager.SetActiveTab: toggle at index {index} is null");
            return;
        }

        activeTabIndex = index;

        try
        {
            for (int i = 0; i < count; i++)
            {
                var other = tabBar.GetToggle(i);
                if (other is not null && other != toggle && other.isOn)
                {
                    Plugin.Logger?.LogInfo($"TabManager.SetActiveTab: turning off {other.name} (index {i})");
                    other.m_IsOn = false;
                }
            }

            Plugin.Logger?.LogInfo($"TabManager.SetActiveTab: turning on {toggle.name} (index {index})");
            toggle.m_IsOn = true;

            Plugin.Logger?.LogInfo($"TabManager.SetActiveTab: calling OnActiveToggleChanged for {toggle.name}");
            activationController.OnActiveToggleChanged(toggle);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"TabManager.SetActiveTab: unexpected exception: {ex}");
        }
    }

    public void FinalizeLayout()
    {
        Plugin.Logger?.LogInfo("TabManager.FinalizeLayout");
        foreach (var content in GetAllContentPanels())
        {
            var rect = content.GetComponent<RectTransform>();
            if (rect is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        if (uiFinder.TabSwitch is not null && m1ToggleGroup is not null)
        {
            var tabSwitchRect = uiFinder.TabSwitch.GetComponent<RectTransform>();
            if (tabSwitchRect is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(tabSwitchRect);

            tabBar.RefreshSize();
            tabBar.Rebuild(m1ToggleGroup);

            var activeToggle = FindActiveToggleInGroup();
            activeTabIndex = FindToggleIndex(activeToggle);
            tabBar.ScrollTo(activeToggle);
        }
        else
        {
            Plugin.Logger?.LogWarning($"TabManager.FinalizeLayout skipped: TabSwitch={(uiFinder.TabSwitch != null)}, m1ToggleGroup={(m1ToggleGroup != null)}");
        }
    }

    private M1Toggle? FindActiveToggleInGroup() =>
        tabBar.GetActiveToggle();

    private int FindToggleIndex(M1Toggle? toggle)
    {
        if (toggle is null)
            return 0;

        var ring = tabBar.Ring;
        for (int i = 0; i < ring.Count; i++)
        {
            if (ring[i] == toggle)
                return i;
        }

        return 0;
    }

    private void AttachArrowListeners()
    {
        if (m1ToggleGroup is null)
        {
            Plugin.Logger?.LogWarning("AttachArrowListeners: m1ToggleGroup is null");
            return;
        }

        var left = m1ToggleGroup.m_Left;
        var right = m1ToggleGroup.m_Right;

        Plugin.Logger?.LogInfo($"AttachArrowListeners: left={(left?.name ?? "null")}, right={(right?.name ?? "null")}");

        if (left is not null)
        {
            left.onClick.RemoveAllListeners();
            Action leftClick = () => ShiftActiveTab(-1);
            left.onClick.AddListener(leftClick);
            Plugin.Logger?.LogInfo($"AttachArrowListeners: attached left listener to {left.name}");
        }

        if (right is not null)
        {
            right.onClick.RemoveAllListeners();
            Action rightClick = () => ShiftActiveTab(1);
            right.onClick.AddListener(rightClick);
            Plugin.Logger?.LogInfo($"AttachArrowListeners: attached right listener to {right.name}");
        }
    }

    private void DetachArrowListeners()
    {
        if (m1ToggleGroup is null)
            return;

        m1ToggleGroup.m_Left?.onClick.RemoveAllListeners();
        m1ToggleGroup.m_Right?.onClick.RemoveAllListeners();
    }

    private void ShiftActiveTab(int delta)
    {
        Plugin.Logger?.LogInfo($"ShiftActiveTab: delta={delta}, current={activeTabIndex}");

        var count = tabBar.RingCount;
        if (count == 0)
        {
            Plugin.Logger?.LogWarning("ShiftActiveTab: ring count is 0");
            return;
        }

        SetActiveTab(activeTabIndex + delta);
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
