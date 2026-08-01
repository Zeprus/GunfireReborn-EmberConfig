namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using EmberConfig.Core;
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
    private readonly TabBarController tabBar;

    private int activeTabIndex;

    public TabManager(UIFinder uiFinder)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        tabBar = new TabBarController(nativeResolver);
        activationController = new TabActivationController(customTabs, nativeResolver, uiFinder);
        customTabFactory = new CustomTabFactory(uiFinder);
        tabBar.OnTabSelected += OnTabSelected;
    }

    public string? CurrentCustomTab => activationController.CurrentCustomTab;

    /// <summary>
    /// Returns the existing content panel for a tab, or <c>null</c> if it does not exist.
    /// </summary>
    public Transform? GetContentForTab(string tabName)
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

        return null;
    }

    /// <summary>
    /// Returns the existing content panel for a tab, creating a custom one if needed.
    /// </summary>
    public Transform? GetOrCreateContentForTab(string tabName)
    {
        var existing = GetContentForTab(tabName);
        if (existing is not null)
            return existing;

        return CreateCustomTab(tabName);
    }

    private Transform? CreateCustomTab(string tabName)
    {
        var normalized = Normalize(tabName);

        if (NativeTabResolver.IsNativeTab(normalized))
            return null;

        if (tabBar.Content is null || uiFinder.Viewport is null || uiFinder.Style is null)
        {
            Plugin.Logger?.LogWarning($"EmberConfig: cannot create custom tab '{normalized}' because the settings UI is not ready.");
            return null;
        }

        var newTab = customTabFactory.Create(normalized, tabBar.Content, uiFinder.Style.Tab);
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

    public string? GetActiveTabName()
    {
        var active = tabBar.GetActiveToggle();
        if (active is null)
            return null;

        if (customTabs.TryGetName(active, out var customName))
            return customName;

        if (nativeResolver.TryGetContentName(active, out var contentName)
            && NativeTabResolver.TryGetNativeTabName(contentName, out var nativeName))
        {
            return nativeName;
        }

        return null;
    }

    public IReadOnlyList<string> GetOrderedTabNames()
    {
        var registry = SettingsRegistry.Current;
        if (registry is null)
            return new List<string>();

        var all = registry.GetTabs().ToList();
        var native = all
            .Where(NativeTabResolver.IsNativeTab)
            .OrderBy(NativeTabResolver.GetNativeTabOrder)
            .ToList();

        var custom = all
            .Where(t => !NativeTabResolver.IsNativeTab(t) && (IsAlwaysVisibleTab(t) || HasVisibleEntries(t)))
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return native.Concat(custom).ToList();
    }

    public void OnUIReady()
    {
        if (uiFinder.TabSwitch is null || uiFinder.Style is null)
        {
            Plugin.Logger?.LogWarning($"TabManager.OnUIReady skipped: TabSwitch={(uiFinder.TabSwitch != null)}, Style={(uiFinder.Style != null)}");
            return;
        }

        var toggleGroup = uiFinder.TabSwitch.GetComponent<M1ToggleGroup>();

        var tabStyle = uiFinder.Style.Tab ?? TabStyle.Fallback(uiFinder.Style.Row.Title);
        tabBar.Initialize(uiFinder.TabSwitch, uiFinder.TabSwitch.parent, tabStyle);

        foreach (var tab in GetOrderedTabNames())
            GetOrCreateContentForTab(tab);

        tabBar.Rebuild();

        var activeToggle = tabBar.GetActiveToggle();
        activeTabIndex = tabBar.GetActiveIndex();
        activationController.OnActiveToggleChanged(activeToggle);
        tabBar.ScrollToStart();
        tabBar.AttachArrowButtons(toggleGroup);
    }

    public void OnPanelClosed()
    {
        customTabs.DestroyAll();
        nativeResolver.Clear();
        tabBar.Reset();
        activationController.ClearCurrentCustomTab();
        activeTabIndex = 0;
    }

    public void ValidateActiveTab()
    {
        if (tabBar.RingCount == 0)
            return;

        var activeToggle = tabBar.GetActiveToggle();
        var activeIndex = tabBar.GetActiveIndex();
        if (activeIndex == activeTabIndex)
            return;

        activeTabIndex = activeIndex;
        activationController.OnActiveToggleChanged(activeToggle);
        tabBar.ScrollTo(activeToggle);
    }

    public void Update(float deltaTime)
    {
        tabBar.Update(deltaTime);
    }

    private void OnTabSelected(int index)
    {
        if (index == activeTabIndex)
            return;

        var toggle = tabBar.GetToggle(index);
        if (toggle is null)
            return;

        activeTabIndex = index;
        activationController.OnActiveToggleChanged(toggle);
        tabBar.ScrollTo(toggle);
    }

    public void FinalizeLayout()
    {
        foreach (var content in GetAllContentPanels())
        {
            var rect = content.GetComponent<RectTransform>();
            if (rect is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        RemoveHiddenCustomTabs();

        if (uiFinder.TabSwitch is not null && tabBar.Content is not null)
        {
            tabBar.Rebuild();

            var activeToggle = tabBar.GetActiveToggle();
            activeTabIndex = tabBar.GetActiveIndex();
            activationController.OnActiveToggleChanged(activeToggle);
        }
        else
        {
            Plugin.Logger?.LogWarning($"TabManager.FinalizeLayout skipped: TabSwitch={(uiFinder.TabSwitch != null)}, Content={(tabBar.Content != null)}");
        }
    }

    private static bool IsAlwaysVisibleTab(string tab) =>
        string.Equals(tab, VisibilityStore.VisibilityTab, StringComparison.OrdinalIgnoreCase);

    private static bool HasVisibleEntries(string tab)
    {
        var registry = SettingsRegistry.Current;
        if (registry is null)
            return false;

        return registry.GetByTab(tab).Any(entry =>
            !VisibilityStore.IsInitialized || VisibilityStore.Current.IsVisible(entry.ModName, tab));
    }

    private void RemoveHiddenCustomTabs()
    {
        var visibleTabs = new HashSet<string>(GetOrderedTabNames(), StringComparer.OrdinalIgnoreCase);

        foreach (var tab in customTabs.All.ToList())
        {
            if (!customTabs.TryGetName(tab.Toggle, out var tabName) || !visibleTabs.Contains(tabName))
                RemoveCustomTab(tabName ?? tab.Toggle.gameObject.name);
        }
    }

    private void RemoveCustomTab(string tabName)
    {
        var normalized = Normalize(tabName);
        if (!customTabs.TryGet(normalized, out var tab))
            return;

        tab.Toggle.gameObject.SetActive(false);
        tab.Toggle.transform.SetParent(null, false);
        tab.Content.gameObject.SetActive(false);
        tab.Content.SetParent(null, false);

        UnityEngine.Object.Destroy(tab.Toggle.gameObject);
        UnityEngine.Object.Destroy(tab.Content.gameObject);
        customTabs.Unregister(normalized);
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
