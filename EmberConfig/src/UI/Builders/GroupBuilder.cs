namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and caches group containers and sub-group headers inside a tab content panel.
/// </summary>
internal sealed class GroupBuilder
{
    private readonly UIFinder uiFinder;
    private readonly Dictionary<string, Transform> groupContainers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> createdSubHeaders = new(StringComparer.OrdinalIgnoreCase);

    public GroupBuilder(UIFinder uiFinder)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
    }

    /// <summary>
    /// Clears cached groups and sub-group headers. Called before a full rebuild.
    /// </summary>
    public void Clear()
    {
        groupContainers.Clear();
        createdSubHeaders.Clear();
    }

    /// <summary>
    /// Returns an existing group container or creates a new one under <paramref name="content"/>.
    /// </summary>
    /// <param name="content">The tab content panel.</param>
    /// <param name="group">The group name.</param>
    /// <returns>The group container transform.</returns>
    public Transform GetOrCreateGroupContainer(Transform content, string? group)
    {
        if (string.IsNullOrWhiteSpace(group))
            return content;

        var key = GetGroupContainerKey(content, group);
        if (groupContainers.TryGetValue(key, out var existing))
            return existing;

        var catalog = uiFinder.Style ?? throw new InvalidOperationException("StyleCatalog not captured.");
        var container = GroupContainerBuilder.Build($"SL_Group_{group}", group, catalog.Row, catalog.GroupHeader, content);
        groupContainers[key] = container;
        return container;
    }

    /// <summary>
    /// Adds a sub-group header under <paramref name="groupContainer"/> if one has not
    /// already been created for the given <paramref name="group"/> and <paramref name="subGroup"/>.
    /// </summary>
    /// <param name="groupContainer">The parent group container.</param>
    /// <param name="group">The group name.</param>
    /// <param name="subGroup">The sub-group name.</param>
    public void EnsureSubGroupHeader(Transform groupContainer, string group, string subGroup)
    {
        if (!createdSubHeaders.Add(GetSubGroupHeaderKey(groupContainer, group, subGroup))) return;

        var catalog = uiFinder.Style;
        if (catalog is null) return;

        var groupStyle = catalog.GroupHeader;

        var go = new GameObject($"SL_SubHeader_{subGroup}");
        var rect = (RectTransform)go.AddComponent<RectTransform>();
        rect.SetParent(groupContainer, false);

        go.AddComponent<CanvasRenderer>();
        var text = go.AddComponent<TextMeshProUGUI>();
        TextAppearanceApplier.Apply(text, groupStyle.Header, $"    {subGroup}", TextAlignmentOptions.Left);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = groupStyle.SubGroupHeaderHeight;

        go.SetActive(true);
    }

    private static string GetGroupContainerKey(Transform content, string group)
        => $"{content.GetInstanceID()}::{group}";

    private static string GetSubGroupHeaderKey(Transform groupContainer, string group, string subGroup)
        => $"{groupContainer.GetInstanceID()}::{group}::{subGroup}";
}
