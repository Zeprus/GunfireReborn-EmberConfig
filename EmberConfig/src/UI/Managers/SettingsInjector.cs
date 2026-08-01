namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EmberConfig.Core;
using UnityEngine;
using UnityEngine.UI;

internal sealed class SettingsInjector
{
    private const float BuildBudgetMs = 4f;
    private const int MinRowsPerFrame = 1;

    private readonly SettingsRegistry registry;
    private readonly TabManager tabManager;
    private readonly RowFactory rowFactory;
    private readonly UIFinder uiFinder;
    private readonly GroupBuilder groupBuilder;
    private readonly List<ISettingRow> rows = new();
    private readonly Queue<BuildJob> buildQueue = new();
    private readonly Stopwatch buildStopwatch = new();
    private readonly ConfirmationCoverMask resetConfirmation = new();

    private bool isRebuilding;
    private Transform? currentBuildContent;
    private string? currentGroup;
    private string? currentSubGroup;
    private Transform? currentGroupContainer;
    private string? lastDesc;

    internal SettingsInjector(SettingsRegistry registry, TabManager tabManager, RowFactory rowFactory, UIFinder uiFinder)
    {
        this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        this.tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        this.rowFactory = rowFactory ?? throw new ArgumentNullException(nameof(rowFactory));
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
        this.groupBuilder = new GroupBuilder(uiFinder);
    }

    public bool IsCapturing => rows.Any(r => r.IsCapturing);

    public bool IsRebuilding => isRebuilding;

    public void UpdateRows()
    {
        ISettingRow? hovered = null;
        ISettingRow? capturing = null;
        foreach (var row in rows)
        {
            row.Update();
            row.UpdateHover();
            if (row.IsHovered)
                hovered = row;
            if (row.IsCapturing)
                capturing = row;
        }

        var descText = uiFinder.Style?.Row.DescriptionText;
        if (descText is not null)
        {
            if (hovered is not null)
            {
                lastDesc = hovered.Description;
                descText.text = lastDesc;
            }
            else if (descText.text == lastDesc)
            {
                descText.text = string.Empty;
                lastDesc = null;
            }
        }
    }

    public void StartRebuild(string? activeTabName)
    {
        if (!uiFinder.IsReady)
            return;

        Clear();

        var orderedTabs = tabManager.GetOrderedTabNames().ToList();
        if (activeTabName is not null)
        {
            var prioritized = new List<string> { activeTabName };
            prioritized.AddRange(orderedTabs.Where(t => !string.Equals(t, activeTabName, StringComparison.OrdinalIgnoreCase)));
            orderedTabs = prioritized;
        }

        foreach (var tab in orderedTabs)
        {
            var content = tabManager.GetOrCreateContentForTab(tab);
            if (content is null)
                continue;

            var sorted = registry
                .GetByTab(tab)
                .ToList()
                .Where(entry => !VisibilityStore.IsInitialized || VisibilityStore.Current.IsVisible(entry.ModName, tab))
                .Select((entry, index) => (entry, index))
                .OrderBy(x => x.entry.Location.Group ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.entry.Location.Group is null ? string.Empty : (x.entry.Location.SubGroup ?? string.Empty), StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.index)
                .Select(x => x.entry)
                .ToList();

            foreach (var entry in sorted)
                buildQueue.Enqueue(new BuildJob(entry, content));
        }

        isRebuilding = true;
    }

    public void BuildNextBatch()
    {
        if (!isRebuilding)
            return;

        if (buildQueue.Count == 0)
        {
            isRebuilding = false;
            RefreshTabBarAndResetButton();
            return;
        }

        buildStopwatch.Restart();
        int processed = 0;

        while (buildQueue.Count > 0)
        {
            if (processed >= MinRowsPerFrame && buildStopwatch.Elapsed.TotalMilliseconds >= BuildBudgetMs)
                break;

            var job = buildQueue.Dequeue();
            ProcessJob(job);
            processed++;
        }

        if (buildQueue.Count == 0)
        {
            isRebuilding = false;
            RefreshTabBarAndResetButton();
        }
    }

    public void RefreshVisibility(string modName, string tabName)
    {
        if (!uiFinder.IsReady || isRebuilding)
            return;

        var visible = VisibilityStore.Current.IsVisible(modName, tabName);

        SetRowsActiveForMod(modName, tabName, visible);

        var content = tabManager.GetContentForTab(tabName);
        if (visible && content is null)
            content = tabManager.GetOrCreateContentForTab(tabName);

        if (content is null && !visible)
        {
            RefreshTabBarAndResetButton();
            return;
        }

        if (content is null)
            return;

        var activeTabBefore = tabManager.GetActiveTabName();
        ScrollPreserver.Preserve(uiFinder.ScrollRect, activeTabBefore, tabManager.GetActiveTabName, () =>
        {
            if (visible)
                BuildMissingRowsForMod(modName, tabName, content);

            if (content.GetComponent<RectTransform>() is RectTransform rect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            RefreshTabBarAndResetButton();

            if (tabManager.GetContentForTab(tabName) is null)
                RemoveUnboundRowsForTab(tabName);
        });
    }

    public void Clear()
    {
        foreach (var row in rows)
            row.Unbind();
        rows.Clear();
        lastDesc = null;
        groupBuilder.Clear();

        buildQueue.Clear();
        isRebuilding = false;
        currentBuildContent = null;
        currentGroup = null;
        currentSubGroup = null;
        currentGroupContainer = null;

        foreach (var content in tabManager.GetAllContentPanels())
        {
            if (content == null)
                continue;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child.name.StartsWith("SL_", StringComparison.OrdinalIgnoreCase))
                    UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    private void ProcessJob(BuildJob job)
    {
        var content = job.Content;
        if (content != currentBuildContent)
        {
            currentBuildContent = content;
            currentGroup = null;
            currentSubGroup = null;
            currentGroupContainer = null;
        }

        var loc = job.Entry.Location;

        if (!string.Equals(currentGroup, loc.Group, StringComparison.OrdinalIgnoreCase))
        {
            currentGroupContainer = PlaceRowInGroup(job.Entry, content);
            currentGroup = loc.Group;
            currentSubGroup = loc.SubGroup;
        }
        else if (!string.Equals(currentSubGroup, loc.SubGroup, StringComparison.OrdinalIgnoreCase))
        {
            currentGroupContainer = PlaceRowInGroup(job.Entry, content);
            currentSubGroup = loc.SubGroup;
        }

        var parent = currentGroupContainer ?? content;
        var row = rowFactory.CreateRow(job.Entry, parent);
        if (row is null)
            return;

        rows.Add(row);
        row.Bind(job.Entry);
    }

    private Transform PlaceRowInGroup(ISettingEntry entry, Transform content)
    {
        var groupContainer = groupBuilder.GetOrCreateGroupContainer(content, entry.Location.Group);

        if (entry.Location.SubGroup is not null && !string.IsNullOrWhiteSpace(entry.Location.Group))
        {
            groupBuilder.EnsureSubGroupHeader(
                groupContainer ?? content,
                entry.Location.Group,
                entry.Location.SubGroup,
                noIndent: entry.Location is { Tab: VisibilityStore.VisibilityTab, Group: VisibilityStore.VisibilityGroup });
        }

        return groupContainer ?? content;
    }

    private void SetRowsActiveForMod(string modName, string tabName, bool visible)
    {
        foreach (var row in rows.ToList())
        {
            if (row.Entry is null)
                continue;

            if (string.Equals(row.Entry.ModName, modName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row.Entry.Location.Tab, tabName, StringComparison.OrdinalIgnoreCase))
            {
                row.GameObject.SetActive(visible);
            }
        }
    }

    private void BuildMissingRowsForMod(string modName, string tabName, Transform content)
    {
        var existingEntries = new HashSet<ISettingEntry>(rows.Select(r => r.Entry).OfType<ISettingEntry>(), ReferenceEqualityComparer.Instance);

        var missing = registry.GetByTab(tabName)
            .Where(entry => string.Equals(entry.ModName, modName, StringComparison.OrdinalIgnoreCase) &&
                            VisibilityStore.Current.IsVisible(entry.ModName, tabName))
            .Where(entry => !existingEntries.Contains(entry))
            .ToList();

        foreach (var entry in missing)
        {
            var parent = PlaceRowInGroup(entry, content);
            var row = rowFactory.CreateRow(entry, parent);
            if (row is null)
                continue;

            row.GameObject.SetActive(true);
            rows.Add(row);
            row.Bind(entry);
        }
    }

    private void RefreshTabBarAndResetButton()
    {
        var content = tabManager.GetOrCreateContentForTab(VisibilityStore.VisibilityTab);
        if (content is null)
            return;

        var style = uiFinder.Style;
        if (style is null)
            return;

        Action onReset = () => VisibilityStore.Current.ResetAllVisibility();
        ResetButtonBuilder.Ensure(content, style, onReset, resetConfirmation);
        tabManager.FinalizeLayout();
    }

    private void RemoveUnboundRowsForTab(string tabName)
    {
        foreach (var row in rows.ToList())
        {
            if (row.Entry is not null && string.Equals(row.Entry.Location.Tab, tabName, StringComparison.OrdinalIgnoreCase))
            {
                row.Unbind();
                rows.Remove(row);
            }
        }
    }

    private readonly record struct BuildJob(ISettingEntry Entry, Transform Content);
}
