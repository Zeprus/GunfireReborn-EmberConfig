namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EmberConfig.Core;
using TMPro;
using UnityEngine;

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
            tabManager.FinalizeLayout();
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
            tabManager.FinalizeLayout();
        }
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
            currentGroupContainer = groupBuilder.GetOrCreateGroupContainer(content, loc.Group);
            currentGroup = loc.Group;
            currentSubGroup = null;
        }

        if (!string.Equals(currentSubGroup, loc.SubGroup, StringComparison.OrdinalIgnoreCase))
        {
            if (loc.SubGroup is not null && !string.IsNullOrWhiteSpace(loc.Group))
                groupBuilder.EnsureSubGroupHeader(currentGroupContainer ?? content, loc.Group, loc.SubGroup, noIndent: loc is { Tab: VisibilityStore.VisibilityTab, Group: VisibilityStore.VisibilityGroup });

            currentSubGroup = loc.SubGroup;
        }

        var parent = currentGroupContainer ?? content;
        var row = rowFactory.CreateRow(job.Entry, parent);
        if (row is null)
            return;

        rows.Add(row);
        row.Bind(job.Entry);
    }

    private readonly record struct BuildJob(ISettingEntry Entry, Transform Content);
}
