namespace EmberConfig.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TabBarScrollAnimator
{
    private const float ScrollSpeed = 8f;

    private ScrollRect? scrollRect;
    private RectTransform? content;
    private RectTransform? viewportRect;
    private TabButtonCollection? buttons;

    private int lastActiveIndex = -1;
    private bool isTransitioning;
    private float targetNormalized;

    public void Initialize(ScrollRect? scrollRect, RectTransform? content, RectTransform? viewportRect, TabButtonCollection buttons)
    {
        this.scrollRect = scrollRect;
        this.content = content;
        this.viewportRect = viewportRect;
        this.buttons = buttons ?? throw new ArgumentNullException(nameof(buttons));
    }

    public void Reset()
    {
        lastActiveIndex = -1;
        isTransitioning = false;
        targetNormalized = 0f;
    }

    public void ScrollTo(M1Toggle? activeToggle)
    {
        if (activeToggle is null || scrollRect is null || content is null || viewportRect is null || buttons is null)
            return;

        int index = buttons.GetIndex(activeToggle);
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

        float leftPadding = content.GetComponent<HorizontalLayoutGroup>()?.padding.left ?? 0f;
        float targetLeftEdge = Mathf.Max(0f, activeLeft - leftPadding);
        targetNormalized = TabBarLayout.ComputeHorizontalNormalizedPosition(viewportWidth, contentWidth, targetLeftEdge);

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

    public void ScrollToStart()
    {
        if (scrollRect is null)
            return;

        scrollRect.horizontalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;
        isTransitioning = false;
        lastActiveIndex = 0;
    }

    public void Update(float deltaTime)
    {
        if (content is null || scrollRect is null || viewportRect is null || !isTransitioning)
            return;

        float current = scrollRect.horizontalNormalizedPosition;
        float newPos = Mathf.MoveTowards(current, targetNormalized, ScrollSpeed * deltaTime);
        scrollRect.horizontalNormalizedPosition = newPos;
        scrollRect.velocity = Vector2.zero;

        if (Mathf.Approximately(newPos, targetNormalized))
            isTransitioning = false;
    }
}
