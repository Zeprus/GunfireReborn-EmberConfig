namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
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

    public void Initialize(Transform tabSwitch, Transform tabContainer, TabStyle style)
    {
        this.style = style;
        view.Initialize(tabSwitch, tabContainer);
        buttons.Initialize(view.Content);
        scroll.Initialize(view.ScrollRect, view.Content, view.Viewport, buttons);
        visuals.Initialize(style, buttons);
        navigator.Initialize(buttons, style, index => SelectTab(index));
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
        float uniformWidth = view.ComputeUniformTabWidth(infos.Count, style);
        if (uniformWidth > 0f && style.HasValue)
            style = style.Value with { Width = uniformWidth };

        buttons.BuildNativeTabs(infos, view.Content, style);
        buttons.RebuildFromContent(view.Content);
        view.ApplyUniformTabWidth(style);

        var active = buttons.GetActiveToggle();
        if (active is null && buttons.InitialActiveIndex >= 0)
        {
            buttons.SetInitialActive(buttons.InitialActiveIndex);
            active = buttons.GetToggle(buttons.InitialActiveIndex);
        }

        visuals.ApplyVisuals(active);
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

    public void Reset()
    {
        view.Reset();
        buttons.Clear();
        scroll.Reset();
        visuals.Reset();
        navigator.Reset();
        style = null;
    }
}
