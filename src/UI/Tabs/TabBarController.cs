namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// High-level controller for the custom scrollable tab bar.
/// </summary>
internal sealed class TabBarController
{
    private const float ScrollSpeed = 8f;
    private const string ContentName = "SL_TabContent";
    private const string ViewportName = "SL_TabViewport";

    private readonly NativeTabResolver nativeResolver;
    private readonly List<M1Toggle> tabs = new();
    private readonly List<TabVisual> visuals = new();

    private RectTransform? tabSwitchRect;
    private RectTransform? viewportRect;
    private RectTransform? content;
    private Transform? container;
    private ScrollRect? scrollRect;
    private TabStyle? style;

    private int lastActiveIndex = -1;
    private int initialActiveIndex = -1;
    private M1Toggle? lastVisualActive;
    private bool isTransitioning;
    private bool isNotifying;
    private bool nativeTabsBuilt;
    private float targetNormalized;

    public event Action<int>? OnTabSelected;

    public TabBarController(NativeTabResolver nativeResolver)
    {
        this.nativeResolver = nativeResolver ?? throw new ArgumentNullException(nameof(nativeResolver));
    }

    public RectTransform? Content => content;

    public int RingCount => tabs.Count;

    public IReadOnlyList<M1Toggle> Ring => tabs;

    public M1Toggle? GetToggle(int index) =>
        index >= 0 && index < tabs.Count ? tabs[index] : null;

    public M1Toggle? GetActiveToggle()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].isOn)
                return tabs[i];
        }

        return null;
    }

    public void Initialize(Transform tabSwitch, Transform tabContainer, TabStyle style)
    {
        if (tabSwitch is null || tabContainer is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: tabSwitch or tabContainer is null");
            return;
        }

        tabSwitchRect = tabSwitch.GetComponent<RectTransform>();
        if (tabSwitchRect is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: tabSwitchRect is null");
            return;
        }

        this.style = style;

        var parent = tabContainer;
        while (parent is not null && parent.name == ViewportName)
            parent = parent.parent;

        if (parent is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: reached null parent while walking up from viewport");
            return;
        }

        container = parent;

        try
        {
            viewportRect = TabBarViewportFactory.Ensure(tabSwitchRect, parent);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"TabBarViewportFactory.Ensure failed: {ex}");
            return;
        }

        if (viewportRect is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: viewportRect is null after Ensure");
            return;
        }

        content = CreateContent(viewportRect);
        if (content is null)
            return;

        scrollRect = viewportRect.GetComponent<ScrollRect>() ?? viewportRect.gameObject.AddComponent<ScrollRect>();
        ConfigureScrollRect();

        RefreshSize();
        HideSourceTabs();
    }

    public void Rebuild(M1ToggleGroup? toggleGroup)
    {
        _ = toggleGroup; // Retained for call-site compatibility; no longer used.

        if (tabSwitchRect is null || viewportRect is null || content is null || nativeResolver is null || !style.HasValue)
        {
            Plugin.Logger?.LogWarning("TabBarController.Rebuild skipped: dependencies are not ready");
            return;
        }

        HideSourceTabs();

        if (!nativeTabsBuilt)
        {
            var infos = nativeResolver.Scan(tabSwitchRect);
            for (int i = 0; i < infos.Count; i++)
            {
                var info = infos[i];
                var toggle = TabButtonBuilder.Build($"tab_native_{info.ContentName}", info.Label, style.Value, content);
                toggle.transform.SetSiblingIndex(i);
                nativeResolver.Register(toggle, info.ContentName);

                if (info.IsActive)
                    initialActiveIndex = i;
            }

            nativeTabsBuilt = true;
        }

        RebuildTabList();

        var active = GetActiveToggle();
        if (active is null && initialActiveIndex >= 0 && initialActiveIndex < tabs.Count)
        {
            active = tabs[initialActiveIndex];
            active.SetIsOnWithoutNotify(true);
            initialActiveIndex = -1;
        }

        UpdateVisuals(active);
        lastVisualActive = active;

        RefreshSize();
    }

    public void ScrollTo(M1Toggle? activeToggle)
    {
        if (activeToggle is null || scrollRect is null || content is null || viewportRect is null)
            return;

        int index = tabs.IndexOf(activeToggle);
        if (index < 0)
            return;

        var activeRect = activeToggle.GetComponent<RectTransform>();
        if (activeRect is null)
            return;

        float activeLeft = activeRect.anchoredPosition.x - activeRect.rect.width * activeRect.pivot.x;
        float contentWidth = content.rect.width;
        float viewportWidth = viewportRect.rect.width;

        if (contentWidth <= 0f || viewportWidth <= 0f)
            return;

        targetNormalized = TabBarLayout.ComputeHorizontalNormalizedPosition(viewportWidth, contentWidth, activeLeft);

        // Snap on first show, wrap, or far jumps; tween for adjacent next/previous.
        if (lastActiveIndex < 0 || Math.Abs(index - lastActiveIndex) > 1)
        {
            scrollRect.horizontalNormalizedPosition = targetNormalized;
            scrollRect.velocity = Vector2.zero;
            isTransitioning = false;
        }
        else
        {
            isTransitioning = true;
        }

        lastActiveIndex = index;
    }

    public void SelectTab(int index)
    {
        if (tabs.Count == 0)
            return;

        index = TabBarLayout.Mod(index, tabs.Count);
        var currentActiveIndex = GetActiveToggleIndex();
        if (index == currentActiveIndex)
            return;

        var active = tabs[index];

        isNotifying = true;
        for (int i = 0; i < tabs.Count; i++)
            tabs[i].SetIsOnWithoutNotify(i == index);
        isNotifying = false;

        UpdateVisuals(active);
        lastVisualActive = active;

        OnTabSelected?.Invoke(index);
    }

    public void NavigateNext()
    {
        if (tabs.Count == 0)
            return;

        var activeIndex = GetActiveToggleIndex();
        if (activeIndex >= 0)
            WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, tabs[activeIndex].gameObject);

        var start = activeIndex >= 0 ? activeIndex : -1;
        SelectTab(TabBarLayout.Mod(start + 1, tabs.Count));
    }

    public void NavigatePrevious()
    {
        if (tabs.Count == 0)
            return;

        var activeIndex = GetActiveToggleIndex();
        if (activeIndex >= 0)
            WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, tabs[activeIndex].gameObject);

        var start = activeIndex >= 0 ? activeIndex : 0;
        SelectTab(TabBarLayout.Mod(start - 1, tabs.Count));
    }

    public void Update(float deltaTime)
    {
        if (content is null || scrollRect is null || viewportRect is null || !style.HasValue)
            return;

        var active = GetActiveToggle();
        if (active != lastVisualActive)
        {
            UpdateVisuals(active);
            lastVisualActive = active;
        }

        if (!isTransitioning)
            return;

        float current = scrollRect.horizontalNormalizedPosition;
        float newPos = Mathf.MoveTowards(current, targetNormalized, ScrollSpeed * deltaTime);
        scrollRect.horizontalNormalizedPosition = newPos;
        scrollRect.velocity = Vector2.zero;

        if (Mathf.Approximately(newPos, targetNormalized))
            isTransitioning = false;
    }

    public void Reset()
    {
        RestoreSourceTabs();

        if (scrollRect is not null)
        {
            scrollRect.content = null;
            scrollRect.enabled = false;
        }

        if (content is not null)
        {
            UnityEngine.Object.Destroy(content.gameObject);
            content = null;
        }

        tabs.Clear();
        visuals.Clear();
        lastActiveIndex = -1;
        initialActiveIndex = -1;
        lastVisualActive = null;
        isTransitioning = false;
        targetNormalized = 0f;
        nativeTabsBuilt = false;

        tabSwitchRect = null;
        viewportRect = null;
        container = null;
        scrollRect = null;
        style = null;
    }

    private int GetActiveToggleIndex()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].isOn)
                return i;
        }

        return -1;
    }

    private void RebuildTabList()
    {
        tabs.Clear();
        visuals.Clear();

        if (content is null)
            return;

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            if (child is null)
                continue;

            var toggle = child.GetComponent<M1Toggle>();
            if (toggle is null)
                continue;

            tabs.Add(toggle);

            var background = child.Find("Background")?.gameObject;
            var label = child.Find("type_name")?.GetComponent<TextMeshProUGUI>();
            visuals.Add(new TabVisual(toggle, background, label));
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            var capturedToggle = tabs[i];
            var capturedIndex = i;

            capturedToggle.onValueChanged.RemoveAllListeners();
            Action<bool> onToggled = isOn =>
            {
                if (!isOn || isNotifying)
                    return;

                WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, capturedToggle.gameObject);
                SelectTab(capturedIndex);
            };
            capturedToggle.onValueChanged.AddListener(onToggled);
        }
    }

    private void UpdateVisuals(M1Toggle? active)
    {
        if (!style.HasValue)
            return;

        var selected = style.Value.Selected;
        var unselected = style.Value.Unselected;

        foreach (var visual in visuals)
        {
            bool isOn = visual.Toggle == active;

            if (visual.Background is not null && visual.Background.activeSelf != isOn)
                visual.Background.SetActive(isOn);

            if (visual.Label is not null)
                visual.Label.color = isOn ? selected.Color : unselected.Color;
        }
    }

    private RectTransform? CreateContent(RectTransform viewport)
    {
        if (viewport is null)
            return null;

        var go = new GameObject(ContentName);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(viewport, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, viewport.sizeDelta.y);

        _ = go.AddComponent<HorizontalLayoutGroup>();

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        go.SetActive(true);
        return rect;
    }

    private void ConfigureScrollRect()
    {
        if (scrollRect is null || content is null || viewportRect is null)
            return;

        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;
        scrollRect.scrollSensitivity = 1f;
        scrollRect.content = content;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontalNormalizedPosition = 0f;
        scrollRect.enabled = true;
    }

    private void RefreshSize()
    {
        if (content is null || viewportRect is null || tabSwitchRect is null || scrollRect is null)
            return;

        var sourceLayout = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        var targetLayout = content.GetComponent<HorizontalLayoutGroup>();
        if (targetLayout is null)
        {
            targetLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        if (sourceLayout is not null)
        {
            targetLayout.spacing = sourceLayout.spacing;
            targetLayout.padding = new RectOffset(
                sourceLayout.padding.left,
                sourceLayout.padding.right,
                sourceLayout.padding.top,
                sourceLayout.padding.bottom);
            targetLayout.childControlWidth = sourceLayout.childControlWidth;
            targetLayout.childControlHeight = sourceLayout.childControlHeight;
            targetLayout.childForceExpandWidth = sourceLayout.childForceExpandWidth;
            targetLayout.childForceExpandHeight = sourceLayout.childForceExpandHeight;
            targetLayout.childAlignment = sourceLayout.childAlignment;
        }
        else
        {
            targetLayout.spacing = 0f;
            targetLayout.padding = new RectOffset(0, 0, 0, 0);
            targetLayout.childControlWidth = true;
            targetLayout.childControlHeight = true;
            targetLayout.childForceExpandWidth = false;
            targetLayout.childForceExpandHeight = false;
            targetLayout.childAlignment = TextAnchor.UpperLeft;
        }

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter is null)
        {
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        scrollRect.content = content;
        scrollRect.viewport = viewportRect;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
    }

    private void HideSourceTabs()
    {
        if (tabSwitchRect is null)
            return;

        tabSwitchRect.gameObject.SetActive(false);
    }

    private void RestoreSourceTabs()
    {
        if (tabSwitchRect is null)
            return;

        tabSwitchRect.gameObject.SetActive(true);
    }

    private readonly record struct TabVisual(M1Toggle Toggle, GameObject? Background, TextMeshProUGUI? Label);
}
