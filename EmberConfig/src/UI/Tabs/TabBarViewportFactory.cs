namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates or reuses the masked <c>SL_TabViewport</c> that wraps the vanilla
/// <c>tab_switch</c> bar and reparents the bar into it.
/// </summary>
internal static class TabBarViewportFactory
{
    private const string ViewportName = "SL_TabViewport";

    internal static RectTransform? Ensure(RectTransform tabSwitchRect, Transform tabContainer)
    {
        if (tabContainer is null || tabSwitchRect is null)
            return null;

        var container = tabContainer;
        var containerRect = tabContainer.GetComponent<RectTransform>();
        while (container is not null && container.name == ViewportName)
        {
            container = container.parent;
            if (container is not null)
                containerRect = container.GetComponent<RectTransform>();
        }

        if (container is null || containerRect is null)
            return null;

        RectTransform? viewportRect = null;
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (child is not null && child.name == ViewportName)
            {
                viewportRect = child.GetComponent<RectTransform>();
                if (viewportRect is not null)
                    break;
            }
        }

        if (viewportRect is null)
            viewportRect = CreateViewport(container, containerRect, tabSwitchRect);

        if (viewportRect is null)
            return null;

        if (tabSwitchRect.parent != viewportRect)
            ReparentTabSwitch(tabSwitchRect, viewportRect);

        return viewportRect;
    }

    private static RectTransform? CreateViewport(Transform container, RectTransform containerRect, RectTransform tabSwitchRect)
    {
        float leftEdge;
        float rightEdge;

        var leftButton = container.Find("Button")?.GetComponent<RectTransform>();
        var rightButton = container.Find("Button2")?.GetComponent<RectTransform>();
        if (leftButton is not null && rightButton is not null)
        {
            leftEdge = leftButton.anchoredPosition.x + leftButton.sizeDelta.x * leftButton.pivot.x;
            rightEdge = rightButton.anchoredPosition.x - rightButton.sizeDelta.x * (1f - rightButton.pivot.x);
        }
        else
        {
            leftEdge = tabSwitchRect.anchoredPosition.x;
            rightEdge = leftEdge + tabSwitchRect.sizeDelta.x;
        }

        var viewportWidth = Mathf.Max(0f, rightEdge - leftEdge);
        var viewportHeight = tabSwitchRect.sizeDelta.y;
        var viewportTop = tabSwitchRect.anchoredPosition.y;

        var go = new GameObject(ViewportName);
        var viewportRectTransform = go.AddComponent<RectTransform>();
        viewportRectTransform.SetParent(container, false);
        viewportRectTransform.anchorMin = new Vector2(0f, 1f);
        viewportRectTransform.anchorMax = new Vector2(0f, 1f);
        viewportRectTransform.pivot = new Vector2(0f, 1f);
        viewportRectTransform.anchoredPosition = new Vector2(leftEdge, viewportTop);
        viewportRectTransform.sizeDelta = new Vector2(viewportWidth, viewportHeight);

        go.AddComponent<CanvasRenderer>();
        go.AddComponent<RectMask2D>();

        var tabSwitchIndex = tabSwitchRect.GetSiblingIndex();
        viewportRectTransform.SetSiblingIndex(tabSwitchIndex);

        return viewportRectTransform;
    }

    private static void ReparentTabSwitch(RectTransform tabSwitchRect, RectTransform viewport)
    {
        tabSwitchRect.SetParent(viewport, false);

        var layoutGroup = tabSwitchRect.GetComponent<HorizontalLayoutGroup>();
        var paddingLeft = layoutGroup?.padding.left ?? 0;

        tabSwitchRect.anchorMin = new Vector2(0f, 1f);
        tabSwitchRect.anchorMax = new Vector2(0f, 1f);
        tabSwitchRect.pivot = new Vector2(0f, 1f);
        tabSwitchRect.anchoredPosition = new Vector2(-paddingLeft, 0f);
    }
}
