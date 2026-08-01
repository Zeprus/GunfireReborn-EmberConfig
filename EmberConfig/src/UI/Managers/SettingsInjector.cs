namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DYControl;
using EmberConfig.Core;
using TMPro;
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

    private bool isRebuilding;
    private Transform? currentBuildContent;
    private string? currentGroup;
    private string? currentSubGroup;
    private Transform? currentGroupContainer;
    private string? lastDesc;
    private readonly ConfirmationCoverMask resetConfirmation = new();

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
            EnsureResetButton();
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
            EnsureResetButton();
            tabManager.FinalizeLayout();
        }
    }

    private void EnsureResetButton()
    {
        var content = tabManager.GetOrCreateContentForTab(VisibilityStore.VisibilityTab);
        if (content is null)
            return;

        const string buttonName = "VisibilityResetButton";
        const string spacerName = "VisibilityResetSpacer";

        var style = uiFinder.Style;
        if (style is null)
            return;

        var existingButton = content.Find(buttonName);
        var existingSpacer = content.Find(spacerName);

        Transform button;
        Transform? spacer;
        if (existingButton is not null)
        {
            button = existingButton;
            spacer = existingSpacer;
        }
        else
        {
            button = CreateResetButton(content, style);
            spacer = null;
        }

        if (spacer is null)
        {
            var spacerGo = new GameObject(spacerName);
            var spacerRect = (RectTransform)spacerGo.AddComponent<RectTransform>();
            spacerRect.SetParent(content, false);

            var spacerLayout = spacerGo.AddComponent<LayoutElement>();
            spacerLayout.minHeight = 30f;
            spacerLayout.preferredHeight = 30f;

            spacer = spacerGo.transform;
        }

        spacer.SetAsLastSibling();
        button.SetAsLastSibling();
    }

    private Transform CreateResetButton(Transform content, StyleCatalog style)
    {
        const string buttonName = "VisibilityResetButton";

        var go = new GameObject(buttonName);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(content, false);
        style.Row.RowRect.Apply(rect);

        _ = go.AddComponent<CanvasRenderer>();

        var image = go.AddComponent<Image>();
        image.sprite = style.Row.BackgroundSprite;
        image.type = style.Row.BackgroundType;
        image.color = Color.white;

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = style.Row.Height;
        layout.flexibleWidth = 1f;

        var textObj = RowElementBuilder.CreateObject("Text", go.transform);
        RowElementBuilder.SetRect(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        RowElementBuilder.AddText(textObj, style.Row.Title, "Reset Visibility", TextAlignmentOptions.Center);

        var redColorBlock = new ColorBlock
        {
            normalColor = new Color(0.25f, 0.05f, 0.05f, 0.42f),
            highlightedColor = new Color(0.40f, 0.12f, 0.12f, 0.50f),
            pressedColor = new Color(0.20f, 0.04f, 0.04f, 0.42f),
            disabledColor = new Color(0.15f, 0.03f, 0.03f, 0.30f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        var button = go.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = redColorBlock;
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(go.transform);

        var dySelect = go.GetComponent<DYSelect>();
        if (dySelect is not null)
            dySelect.isCurBtn = true;

        Action onResetClick = () =>
        {
            if (uiFinder.Viewport is not null)
            {
                Action confirm = () =>
                {
                    resetConfirmation.Hide();
                    VisibilityStore.Current.ResetAllVisibility();
                };

                Action cancel = () => resetConfirmation.Hide();

                resetConfirmation.Show(uiFinder.Viewport, style,
                    "Reset all visibility settings?",
                    "This will delete every mod visibility setting and restore the default visible state.",
                    confirm, cancel);
            }
            else
            {
                VisibilityStore.Current.ResetAllVisibility();
            }
        };

        button.onClick.AddListener(onResetClick);

        go.SetActive(true);
        return go.transform;
    }

    public void RefreshVisibility(string modName, string tabName)
    {
        if (!uiFinder.IsReady || isRebuilding)
            return;

        var visible = VisibilityStore.Current.IsVisible(modName, tabName);

        // Update active state of existing rows for this mod/tab.
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

        // Get or create the content panel for the affected tab.
        var content = tabManager.GetContentForTab(tabName);
        if (visible && content is null)
            content = tabManager.GetOrCreateContentForTab(tabName);

        if (content is null && !visible)
        {
            tabManager.FinalizeLayout();
            EnsureResetButton();
            return;
        }

        if (content is null)
            return;

        // Preserve scroll if the user is currently on the affected tab.
        var activeTabBefore = tabManager.GetActiveTabName();
        var isActiveTab = string.Equals(activeTabBefore, tabName, StringComparison.OrdinalIgnoreCase);
        float? savedScroll = isActiveTab ? uiFinder.ScrollRect?.verticalNormalizedPosition : null;

        // Build any missing rows if the mod/tab is now visible.
        if (visible)
        {
            var existingEntries = new HashSet<ISettingEntry>(rows.Select(r => r.Entry).OfType<ISettingEntry>(), ReferenceEqualityComparer.Instance);

            var missing = registry.GetByTab(tabName)
                .Where(entry => string.Equals(entry.ModName, modName, StringComparison.OrdinalIgnoreCase) &&
                                VisibilityStore.Current.IsVisible(entry.ModName, tabName))
                .Where(entry => !existingEntries.Contains(entry))
                .ToList();

            foreach (var entry in missing)
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

                var parent = groupContainer ?? content;
                var row = rowFactory.CreateRow(entry, parent);
                if (row is null)
                    continue;

                row.GameObject.SetActive(true);
                rows.Add(row);
                row.Bind(entry);
            }
        }

        // Rebuild the affected tab's layout.
        if (content.GetComponent<RectTransform>() is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        // Sync custom tab buttons and the reset button.
        tabManager.FinalizeLayout();
        EnsureResetButton();

        // If the affected custom tab was removed, unbind and drop the now-destroyed rows.
        if (tabManager.GetContentForTab(tabName) is null)
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

        // Restore scroll if the active tab is still the affected tab.
        if (savedScroll.HasValue &&
            string.Equals(tabManager.GetActiveTabName(), tabName, StringComparison.OrdinalIgnoreCase) &&
            uiFinder.ScrollRect is not null)
        {
            Canvas.ForceUpdateCanvases();
            uiFinder.ScrollRect.verticalNormalizedPosition = savedScroll.Value;
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
