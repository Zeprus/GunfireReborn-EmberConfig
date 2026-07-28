namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using SettingsLib;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// High-level controller for the custom tab bar carousel, indicator, and source tab hiding.
/// </summary>
internal sealed class TabBarController
{
    private const string CarouselName = "SL_TabCarousel";
    private const string IndicatorName = "SL_TabIndicator";

    private RectTransform? tabSwitchRect;
    private RectTransform? viewportRect;
    private RectTransform? carouselParent;
    private RectTransform? indicatorParent;
    private Transform? container;
    private TabCarouselController? carousel;
    private TabIndicatorController? indicator;
    private TabStyle style;

    public event Action<int>? OnTabSelected;

    public void Initialize(Transform tabSwitch, Transform tabContainer, TabStyle style)
    {
        Plugin.Logger?.LogInfo($"TabBarController.Initialize: tabSwitch={tabSwitch?.name}, tabContainer={tabContainer?.name}");
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
        container = tabContainer;

        var parent = tabContainer;
        const string ViewportName = "SL_TabViewport";
        while (parent is not null && parent.name == ViewportName)
        {
            parent = parent.parent;
        }

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

        Plugin.Logger?.LogInfo($"TabBarController.Initialize: viewportRect={viewportRect.name} size={viewportRect.sizeDelta}");

        try
        {
            CreateCarouselParent();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"CreateCarouselParent failed: {ex}");
        }

        try
        {
            CreateIndicatorParent();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"CreateIndicatorParent failed: {ex}");
        }

        if (carouselParent is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: carouselParent is null after CreateCarouselParent");
            carousel = null;
        }
        else
        {
            carousel = new TabCarouselController(carouselParent, style, index => OnTabSelected?.Invoke(index));
        }

        if (indicatorParent is null)
        {
            Plugin.Logger?.LogWarning("TabBarController.Initialize: indicatorParent is null after CreateIndicatorParent");
            indicator = null;
        }
        else
        {
            indicator = new TabIndicatorController(indicatorParent, style);
        }

        Plugin.Logger?.LogInfo($"TabBarController.Initialize: carousel={(carousel != null)}, indicator={(indicator != null)}");
    }

    public void Rebuild(M1ToggleGroup? toggleGroup)
    {
        Plugin.Logger?.LogInfo($"TabBarController.Rebuild: toggleGroup={(toggleGroup != null)}, carousel={(carousel != null)}, indicator={(indicator != null)}");
        if (toggleGroup is null || carousel is null || indicator is null)
        {
            Plugin.Logger?.LogWarning($"TabBarController.Rebuild skipped: toggleGroup={(toggleGroup != null)}, carousel={(carousel != null)}, indicator={(indicator != null)}");
            return;
        }

        HideSourceTabs();
        RefreshSize();

        carousel.Rebuild(toggleGroup);
        indicator.Rebuild(carousel.RingCount);
    }

    public void ScrollTo(M1Toggle? activeToggle)
    {
        if (activeToggle is null || carousel is null)
            return;

        carousel.SetActive(activeToggle);
    }

    public M1Toggle? GetToggle(int index) =>
        carousel?.GetToggle(index);

    public M1Toggle? GetActiveToggle() =>
        carousel?.GetActiveToggle();

    public int RingCount =>
        carousel?.RingCount ?? 0;

    public IReadOnlyList<M1Toggle> Ring =>
        carousel?.Ring ?? new List<M1Toggle>();

    public void Update(float deltaTime)
    {
        if (carousel is null || indicator is null)
            return;

        carousel.Update(deltaTime);
        indicator.Update(carousel.CurrentActive, carousel.RingCount);
    }

    public void Reset()
    {
        if (carouselParent != null)
        {
            UnityEngine.Object.Destroy(carouselParent.gameObject);
            carouselParent = null;
        }

        if (indicatorParent != null)
        {
            UnityEngine.Object.Destroy(indicatorParent.gameObject);
            indicatorParent = null;
        }

        if (tabSwitchRect != null)
            RestoreSourceTabs();

        carousel = null;
        indicator = null;
    }

    public void RefreshSize()
    {
        if (viewportRect is null || tabSwitchRect is null || carousel is null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);

        var layoutGroup = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        var spacing = layoutGroup?.spacing ?? 0f;
        var viewportWidth = viewportRect.sizeDelta.x;
        var viewportHeight = viewportRect.sizeDelta.y;

        Plugin.Logger?.LogInfo($"TabBarController.RefreshSize: viewport={viewportRect.name} size=({viewportWidth}, {viewportHeight}) spacing={spacing}");

        carousel.SetMetrics(viewportWidth, spacing);

        if (carouselParent != null)
        {
            carouselParent.sizeDelta = new Vector2(viewportWidth, style.Height > 0 ? style.Height : viewportHeight);
            carouselParent.anchoredPosition = Vector2.zero;
        }

        if (indicatorParent != null)
        {
            indicatorParent.sizeDelta = new Vector2(viewportWidth, 12f);
            indicatorParent.anchoredPosition = new Vector2(viewportRect.anchoredPosition.x, viewportRect.anchoredPosition.y - viewportHeight - 4f);
        }
    }

    private void CreateCarouselParent()
    {
        if (viewportRect is null)
        {
            Plugin.Logger?.LogWarning("CreateCarouselParent: viewportRect is null");
            return;
        }

        var viewportWidth = viewportRect.sizeDelta.x;
        var viewportHeight = viewportRect.sizeDelta.y;
        Plugin.Logger?.LogInfo($"CreateCarouselParent: creating {CarouselName} under {viewportRect.name} size=({viewportWidth}, {viewportHeight})");

        var go = new GameObject(CarouselName);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(viewportRect, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(viewportWidth, style.Height > 0 ? style.Height : viewportHeight);

        carouselParent = rect;
        Plugin.Logger?.LogInfo($"CreateCarouselParent: created {go.name} with parent={go.transform.parent?.name}");
    }

    private void CreateIndicatorParent()
    {
        if (viewportRect is null || container is null)
        {
            Plugin.Logger?.LogWarning($"CreateIndicatorParent: viewportRect={(viewportRect != null)}, container={(container != null)}");
            return;
        }

        var viewportWidth = viewportRect.sizeDelta.x;
        var viewportHeight = viewportRect.sizeDelta.y;
        Plugin.Logger?.LogInfo($"CreateIndicatorParent: creating {IndicatorName} under {container.name} size=({viewportWidth}, {viewportHeight})");

        var go = new GameObject(IndicatorName);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(container, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        rect.sizeDelta = new Vector2(viewportWidth, 12f);
        rect.anchoredPosition = new Vector2(
            viewportRect.anchoredPosition.x,
            viewportRect.anchoredPosition.y - viewportHeight - 4f);

        rect.SetSiblingIndex(viewportRect.GetSiblingIndex() + 1);

        indicatorParent = rect;
        Plugin.Logger?.LogInfo($"CreateIndicatorParent: created {go.name} with parent={go.transform.parent?.name}");
    }

    private void HideSourceTabs()
    {
        if (tabSwitchRect is null)
            return;

        for (int i = 0; i < tabSwitchRect.childCount; i++)
        {
            var child = tabSwitchRect.GetChild(i);
            if (child != null)
                child.gameObject.SetActive(false);
        }
    }

    private void RestoreSourceTabs()
    {
        if (tabSwitchRect is null)
            return;

        for (int i = 0; i < tabSwitchRect.childCount; i++)
        {
            var child = tabSwitchRect.GetChild(i);
            if (child != null)
                child.gameObject.SetActive(true);
        }
    }
}
