namespace EmberConfig.UI;

using System;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;

internal sealed class TabBarScrollAnimator
{

    private ScrollRect? scrollRect;
    private RectTransform? content;
    private RectTransform? viewportRect;
    private TabButtonCollection? buttons;

    private int lastActiveIndex = -1;
    private bool isTransitioning;
    private float startNormalized;
    private float targetNormalized;
    private float transitionDuration;
    private float elapsed;

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
        startNormalized = 0f;
        targetNormalized = 0f;
        transitionDuration = 0f;
        elapsed = 0f;
    }

    public void ScrollTo(M1Toggle? activeToggle)
    {
        if (activeToggle is null || scrollRect is null || content is null || viewportRect is null || buttons is null)
            return;

        int index = buttons.GetIndex(activeToggle);
        if (index < 0 || index == lastActiveIndex)
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
        float activeCenter = activeLeft + activeRect.rect.width * 0.5f;
        float targetLeftEdge = Max(0f, activeCenter - viewportWidth * 0.5f - leftPadding);
        float computedTarget = TabBarLayout.ComputeHorizontalNormalizedPosition(viewportWidth, contentWidth, targetLeftEdge);

        startNormalized = scrollRect.horizontalNormalizedPosition;
        targetNormalized = computedTarget;
        float distance = Abs(targetNormalized - startNormalized);
        transitionDuration = TabBarScrollEasing.ComputeDuration(EmberConfigSettings.TabScrollAnimationDuration, distance);
        elapsed = 0f;
        isTransitioning = true;
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

        elapsed += deltaTime;
        float t = Clamp(elapsed / transitionDuration, 0f, 1f);
        float eased = TabBarScrollEasing.EaseOutCubic(t);
        float newPos = TabBarScrollEasing.Lerp(startNormalized, targetNormalized, eased);
        scrollRect.horizontalNormalizedPosition = Clamp(newPos, 0f, 1f);
        scrollRect.velocity = Vector2.zero;

        if (t >= 1f)
            isTransitioning = false;
    }
}
