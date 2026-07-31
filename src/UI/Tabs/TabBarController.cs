namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// High-level controller for the custom scrollable tab bar.
/// Delegates viewport, button management, scrolling, visuals, and navigation
/// to focused collaborators.
/// </summary>
internal sealed class TabBarController
{
    private readonly NativeTabResolver nativeResolver;
    private readonly TabBarView view = new();
    private readonly TabButtonCollection buttons;
    private readonly TabBarScrollAnimator scroll = new();
    private readonly TabBarVisuals visuals = new();
    private readonly TabBarNavigator navigator = new();

    private TabStyle? style;
    private int nativeTabCount;

    public event Action<int>? OnTabSelected;

    public TabBarController(NativeTabResolver nativeResolver)
    {
        this.nativeResolver = nativeResolver ?? throw new ArgumentNullException(nameof(nativeResolver));
        this.buttons = new TabButtonCollection(nativeResolver);
        this.buttons.TabSelected += index =>
        {
            visuals.ApplyVisuals(buttons.GetActiveToggle());
            OnTabSelected?.Invoke(index);
        };
    }

    public RectTransform? Content => view.Content;

    public int RingCount => buttons.Count;

    public IReadOnlyList<M1Toggle> Ring => buttons.Toggles;

    public M1Toggle? GetToggle(int index) => buttons.GetToggle(index);

    public M1Toggle? GetActiveToggle() => buttons.GetActiveToggle();

    public int GetActiveIndex() => buttons.GetActiveIndex();

    private static void SortTabButtons(RectTransform? content)
    {
        if (content is null)
            return;

        var children = new List<Transform>();
        for (int i = 0; i < content.childCount; i++)
            children.Add(content.GetChild(i));

        children.Sort(CompareTabOrder);

        for (int i = 0; i < children.Count; i++)
            children[i].SetSiblingIndex(i);
    }

    private static int CompareTabOrder(Transform a, Transform b)
    {
        bool aNative = a.name.StartsWith("tab_native_", StringComparison.OrdinalIgnoreCase);
        bool bNative = b.name.StartsWith("tab_native_", StringComparison.OrdinalIgnoreCase);

        if (aNative && !bNative) return -1;
        if (!aNative && bNative) return 1;

        var aId = GetTabIdentifier(a);
        var bId = GetTabIdentifier(b);

        if (aNative && bNative)
        {
            var aName = NativeTabResolver.TryGetNativeTabName(aId, out var an) ? an : null;
            var bName = NativeTabResolver.TryGetNativeTabName(bId, out var bn) ? bn : null;
            var aOrder = aName is not null ? NativeTabResolver.GetNativeTabOrder(aName) : int.MaxValue;
            var bOrder = bName is not null ? NativeTabResolver.GetNativeTabOrder(bName) : int.MaxValue;
            return aOrder.CompareTo(bOrder);
        }

        return string.Compare(aId, bId, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetTabIdentifier(Transform t)
    {
        var parts = t.name.Split('_', 3);
        return parts.Length > 2 ? parts[2] : t.name;
    }

    public void Initialize(Transform tabSwitch, Transform tabContainer, TabStyle style)
    {
        this.style = style;
        view.Initialize(tabSwitch, tabContainer);
        buttons.Initialize(view.Content);
        scroll.Initialize(view.ScrollRect, view.Content, view.Viewport, buttons);
        visuals.Initialize(style, buttons);
        navigator.Initialize(buttons, style, index => SelectTab(index));
        EmberConfigSettings.TabWidthScalingChanged += OnTabWidthScalingChanged;
        view.HideSourceTabs();
    }

    public void Rebuild()
    {
        if (!view.IsReady)
        {
            Plugin.Logger?.LogWarning("TabBarController.Rebuild skipped: view is not ready");
            return;
        }

        view.HideSourceTabs();

        var infos = nativeResolver.Scan(view.TabSwitch!);
        nativeTabCount = infos.Count;

        buttons.BuildNativeTabs(infos, view.Content, style);
        SortTabButtons(view.Content);
        buttons.RebuildFromContent(view.Content);

        var active = buttons.GetActiveToggle();
        if (active is null && buttons.InitialActiveIndex >= 0)
        {
            buttons.SetInitialActive(buttons.InitialActiveIndex);
            active = buttons.GetToggle(buttons.InitialActiveIndex);
        }

        visuals.ApplyVisuals(active);
        ApplyTabWidth(EmberConfigSettings.TabWidthScaling);
        view.RefreshSize();
    }

    public void ScrollTo(M1Toggle? activeToggle) => scroll.ScrollTo(activeToggle);

    public void ScrollToStart() => scroll.ScrollToStart();

    public void SelectTab(int index) => buttons.SelectTab(index);

    public void NavigateNext() => navigator.NavigateNext();

    public void NavigatePrevious() => navigator.NavigatePrevious();

    public void AttachArrowButtons(M1ToggleGroup? toggleGroup) => navigator.AttachArrowButtons(toggleGroup);

    public void Update(float deltaTime)
    {
        visuals.Update();
        scroll.Update(deltaTime);
    }

    private void ApplyTabWidth(float scaling)
    {
        if (!style.HasValue)
            return;

        var baseWidth = view.ComputeUniformTabWidth(nativeTabCount, style);
        var width = baseWidth * (scaling / 100f);
        var height = style.Value.Height;
        view.ApplyUniformTabWidth(width, height);
    }

    private void OnTabWidthScalingChanged(float scaling)
    {
        if (!view.IsReady || !style.HasValue)
            return;

        ApplyTabWidth(scaling);
        view.RefreshSize();
    }

    public void Reset()
    {
        EmberConfigSettings.TabWidthScalingChanged -= OnTabWidthScalingChanged;
        view.Reset();
        buttons.Clear();
        scroll.Reset();
        visuals.Reset();
        navigator.Reset();
        style = null;
        nativeTabCount = 0;
    }
}
