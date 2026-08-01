namespace EmberConfig.UI;

using System;
using EmberConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TabBarView
{
    private const string ViewportName = "SL_TabViewport";
    private const string ContentName = "SL_TabContent";

    private RectTransform? tabSwitchRect;
    private RectTransform? viewportRect;
    private RectTransform? content;
    private Transform? container;
    private ScrollRect? scrollRect;
    private Action<float>? onScrollSensitivityChanged;

    public RectTransform? Content => content;
    public RectTransform? Viewport => viewportRect;
    public ScrollRect? ScrollRect => scrollRect;
    public RectTransform? TabSwitch => tabSwitchRect;

    public bool IsReady => tabSwitchRect is not null && viewportRect is not null && content is not null && scrollRect is not null;

    public void Initialize(Transform tabSwitch, Transform tabContainer)
    {
        if (tabSwitch is null || tabContainer is null)
        {
            Plugin.Logger?.LogWarning("TabBarView.Initialize: tabSwitch or tabContainer is null");
            return;
        }

        tabSwitchRect = tabSwitch.GetComponent<RectTransform>();
        if (tabSwitchRect is null)
        {
            Plugin.Logger?.LogWarning("TabBarView.Initialize: tabSwitchRect is null");
            return;
        }

        var parent = tabContainer;
        while (parent is not null && parent.name == ViewportName)
            parent = parent.parent;

        if (parent is null)
        {
            Plugin.Logger?.LogWarning("TabBarView.Initialize: reached null parent while walking up from viewport");
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
            Plugin.Logger?.LogWarning("TabBarView.Initialize: viewportRect is null after Ensure");
            return;
        }

        EnsureViewportHitArea(viewportRect);
        content = CreateContent(viewportRect);
        if (content is null)
            return;

        scrollRect = viewportRect.GetComponent<ScrollRect>() ?? viewportRect.gameObject.AddComponent<ScrollRect>();
        ConfigureScrollRect();
        HideSourceTabs();
    }

    public void Reset()
    {
        if (onScrollSensitivityChanged is not null)
        {
            EmberConfigSettings.TabScrollSensitivityChanged -= onScrollSensitivityChanged;
            onScrollSensitivityChanged = null;
        }

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

        tabSwitchRect = null;
        viewportRect = null;
        container = null;
        scrollRect = null;
    }

    public void RefreshSize()
    {
        if (content is null || viewportRect is null || tabSwitchRect is null || scrollRect is null)
            return;

        var sourceLayout = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        var targetLayout = content.GetComponent<HorizontalLayoutGroup>();
        if (targetLayout is null)
            targetLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();

        if (sourceLayout is not null)
        {
            targetLayout.spacing = sourceLayout.spacing;

            int totalHorizontalPadding = sourceLayout.padding.left + sourceLayout.padding.right;
            int halfHorizontalPadding = totalHorizontalPadding / 2;
            targetLayout.padding = new RectOffset(
                halfHorizontalPadding,
                halfHorizontalPadding,
                sourceLayout.padding.top,
                sourceLayout.padding.bottom);

            targetLayout.childAlignment = sourceLayout.childAlignment;
        }
        else
        {
            targetLayout.spacing = 0f;
            targetLayout.padding = new RectOffset(0, 0, 0, 0);
            targetLayout.childAlignment = TextAnchor.UpperLeft;
        }

        targetLayout.childControlWidth = true;
        targetLayout.childControlHeight = true;
        targetLayout.childForceExpandWidth = false;
        targetLayout.childForceExpandHeight = false;

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

    public float ComputeUniformTabWidth(int tabCount, TabStyle? style)
    {
        if (tabCount <= 0 || viewportRect is null || tabSwitchRect is null || !style.HasValue)
            return style?.Width ?? 220f;

        var sourceLayout = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        if (sourceLayout is null)
            return style.Value.Width;

        float viewportWidth = viewportRect.sizeDelta.x;
        float spacing = sourceLayout.spacing;
        float totalHorizontalPadding = sourceLayout.padding.horizontal;

        float width = (viewportWidth - totalHorizontalPadding - spacing * (tabCount - 1)) / tabCount;
        return width > 0f ? width : style.Value.Width;
    }

    public void ApplyUniformTabWidth(float width, float height)
    {
        if (content is null)
            return;

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            if (child is null)
                continue;

            if (child.GetComponent<M1Toggle>() is null)
                continue;

            var rect = child.GetComponent<RectTransform>();
            if (rect is not null)
                rect.sizeDelta = new Vector2(width, height);

            var layout = child.GetComponent<LayoutElement>();
            if (layout is not null)
            {
                layout.minWidth = width;
                layout.preferredWidth = width;
                layout.minHeight = height;
                layout.preferredHeight = height;
            }

            var checkmark = child.Find("Background/Checkmark")?.GetComponent<RectTransform>();
            if (checkmark is not null)
                checkmark.sizeDelta = new Vector2(width, checkmark.sizeDelta.y);
        }

        // Update the layout so each tab's RectTransform has its final size
        // before we decide whether the label fits or needs truncating.
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < content.childCount; i++)
            AdjustTabLabel(content.GetChild(i), width);
    }

    private static void AdjustTabLabel(Transform? tab, float width)
    {
        if (tab is null || tab.GetComponent<M1Toggle>() is null)
            return;

        var typeNameObj = tab.Find("type_name");
        var typeName = typeNameObj?.GetComponent<TextMeshProUGUI>();
        var typeNameRect = typeNameObj?.GetComponent<RectTransform>();
        if (typeName is null || typeNameRect is null)
            return;

        // Ensure the label's own RectTransform is exactly the tab size.
        // TMP's auto-size text container sometimes expands the label object,
        // which breaks both fitting and clipping.
        typeNameRect.anchorMin = Vector2.zero;
        typeNameRect.anchorMax = Vector2.one;
        typeNameRect.pivot = new Vector2(0.5f, 0.5f);
        typeNameRect.anchoredPosition = Vector2.zero;
        typeNameRect.sizeDelta = Vector2.zero;
        typeName.autoSizeTextContainer = false;

        typeName.fontSizeMin = EmberConfigSettings.TabMinFontSize;
        TabButtonBuilder.FitText(typeName, width);
    }

    public void HideSourceTabs()
    {
        if (tabSwitchRect is not null)
            tabSwitchRect.gameObject.SetActive(false);
    }

    public void RestoreSourceTabs()
    {
        if (tabSwitchRect is not null)
            tabSwitchRect.gameObject.SetActive(true);
    }

    private void EnsureViewportHitArea(RectTransform viewport)
    {
        if (viewport is null)
            return;

        var image = viewport.GetComponent<Image>() ?? viewport.gameObject.AddComponent<Image>();
        image.sprite = UIResources.WhiteSprite;
        image.type = Image.Type.Simple;
        image.color = Color.clear;
        image.raycastTarget = true;
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
        scrollRect.scrollSensitivity = EmberConfigSettings.TabScrollSensitivity;
        scrollRect.content = content;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontalNormalizedPosition = 0f;
        scrollRect.enabled = true;

        onScrollSensitivityChanged = value =>
        {
            if (scrollRect is not null)
                scrollRect.scrollSensitivity = value;
        };
        EmberConfigSettings.TabScrollSensitivityChanged += onScrollSensitivityChanged;
    }
}
