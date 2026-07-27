namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Computes and applies the horizontal scroll offset for the tab bar so the
/// active tab is visible and, when possible, centered.
/// </summary>
internal static class TabBarScrollController
{
    internal static void ScrollTo(M1Toggle? activeToggle, RectTransform? tabSwitchRect, RectTransform? viewportRect)
    {
        if (activeToggle is null || tabSwitchRect is null || viewportRect is null)
            return;

        if (!tabSwitchRect || !viewportRect)
            return;

        var viewport = viewportRect.rect;
        var viewportMin = viewport.xMin;
        var viewportMax = viewport.xMax;

        if (!TryGetChildEdges(tabSwitchRect, out var childEdges) || childEdges.Count == 0)
            return;

        var contentMin = childEdges[0].Left;
        var contentMax = childEdges[childEdges.Count - 1].Right;

        var activeRect = activeToggle.GetComponent<RectTransform>();
        if (activeRect is null)
            return;

        float activeMin = 0f;
        float activeMax = 0f;
        bool found = false;
        for (int i = 0; i < childEdges.Count; i++)
        {
            if (childEdges[i].Transform == activeRect)
            {
                activeMin = childEdges[i].Left;
                activeMax = childEdges[i].Right;
                found = true;
                break;
            }
        }

        if (!found)
            return;

        var contentWidth = contentMax - contentMin;
        var contentFits = contentWidth <= viewport.width;

        var targetActiveMin = contentFits ? contentMin : activeMin;
        var targetActiveMax = contentFits ? contentMax : activeMax;

        var currentOffset = tabSwitchRect.localPosition.x;
        var target = TabBarLayout.ComputeScrollOffset(
            viewportMin,
            viewportMax,
            contentMin,
            contentMax,
            targetActiveMin,
            targetActiveMax,
            currentOffset,
            recenterIfVisible: true);

        if (MathF.Abs(target - currentOffset) < 0.01f)
            return;

        var localPosition = tabSwitchRect.localPosition;
        localPosition.x = target;
        tabSwitchRect.localPosition = localPosition;
    }

    private static bool TryGetChildEdges(RectTransform tabSwitchRect, out List<ChildEdge> edges)
    {
        edges = new List<ChildEdge>();

        for (int i = 0; i < tabSwitchRect.childCount; i++)
        {
            var child = tabSwitchRect.GetChild(i);
            if (child is null)
                continue;

            var rect = child.GetComponent<RectTransform>();
            if (rect is null)
                continue;

            edges.Add(new ChildEdge(rect, rect.localPosition.x + rect.rect.xMin, rect.localPosition.x + rect.rect.xMax));
        }

        edges.Sort((a, b) => a.Left.CompareTo(b.Left));
        return edges.Count > 0;
    }

    private readonly struct ChildEdge
    {
        public RectTransform Transform { get; }
        public float Left { get; }
        public float Right { get; }

        public ChildEdge(RectTransform transform, float left, float right)
        {
            Transform = transform;
            Left = left;
            Right = right;
        }
    }
}
