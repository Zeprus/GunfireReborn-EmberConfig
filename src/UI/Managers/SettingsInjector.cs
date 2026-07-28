namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using EmberConfig.Core;
using TMPro;
using UnityEngine;

internal sealed class SettingsInjector
{
    private readonly SettingsRegistry registry;
    private readonly TabManager tabManager;
    private readonly RowFactory rowFactory;
    private readonly UIFinder uiFinder;
    private readonly GroupBuilder groupBuilder;
    private readonly List<ISettingRow> rows = new();
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

    public void Rebuild()
    {
        if (!uiFinder.IsReady)
            return;

        Clear();

        foreach (var tab in registry.GetTabs())
        {
            var content = tabManager.GetOrCreateContentForTab(tab);
            if (content is null)
                continue;

            BuildTab(registry.GetByTab(tab), content);
        }

        tabManager.FinalizeLayout();
    }

    public void Clear()
    {
        foreach (var row in rows)
            row.Unbind();
        rows.Clear();
        lastDesc = null;
        groupBuilder.Clear();

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

    private void BuildTab(IEnumerable<ISettingEntry> entries, Transform content)
    {
        string? currentGroup = null;
        string? currentSubGroup = null;
        Transform? groupContainer = null;

        foreach (var entry in entries)
        {
            var loc = entry.Location;

            if (!string.Equals(currentGroup, loc.Group, StringComparison.OrdinalIgnoreCase))
            {
                groupContainer = groupBuilder.GetOrCreateGroupContainer(content, loc.Group);
                currentGroup = loc.Group;
                currentSubGroup = null;
            }

            if (!string.Equals(currentSubGroup, loc.SubGroup, StringComparison.OrdinalIgnoreCase))
            {
                if (loc.SubGroup is not null)
                    groupBuilder.EnsureSubGroupHeader(groupContainer ?? content, loc.Group, loc.SubGroup);

                currentSubGroup = loc.SubGroup;
            }

            var parent = groupContainer ?? content;
            var row = rowFactory.CreateRow(entry, parent);
            if (row is null) continue;
            rows.Add(row);
            row.Bind(entry);
        }
    }
}
