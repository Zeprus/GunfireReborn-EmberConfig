namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual carousel of tab slots: mapping them to the real
/// <see cref="M1ToggleGroup"/>, animating positions, and handling wrap-around.
/// </summary>
internal sealed class TabCarouselController
{
    private const float TransitionDuration = 0.15f;

    private readonly RectTransform carouselParent;
    private readonly TabStyle style;
    private readonly Action<int> onTabSelected;
    private readonly List<TabCarouselSlot> slots = new();
    private readonly List<M1Toggle> ring = new();
    private readonly List<string> labels = new();

    private float slotWidth;
    private float slotHeight;
    private float step;
    private float currentActive;
    private float targetActive;
    private int targetMod = -1;
    private bool isTransitioning;

    public TabCarouselController(RectTransform carouselParent, TabStyle style, Action<int> onTabSelected)
    {
        this.carouselParent = carouselParent ?? throw new ArgumentNullException(nameof(carouselParent));
        this.style = style;
        this.onTabSelected = onTabSelected ?? throw new ArgumentNullException(nameof(onTabSelected));
    }

    public float CurrentActive => currentActive;

    public int RingCount => ring.Count;

    public IReadOnlyList<M1Toggle> Ring => ring;

    public M1Toggle? GetToggle(int index) =>
        index >= 0 && index < ring.Count ? ring[index] : null;

    public M1Toggle? GetActiveToggle()
    {
        foreach (var toggle in ring)
        {
            if (toggle is not null && toggle.isOn)
                return toggle;
        }

        return null;
    }

    private List<M1Toggle> CollectToggles(M1ToggleGroup? group)
    {
        if (group is not null && group.m_Toggles is not null && group.m_Toggles.Count > 0)
        {
            var list = new List<M1Toggle>(group.m_Toggles.Count);
            for (int i = 0; i < group.m_Toggles.Count; i++)
            {
                var toggle = group.m_Toggles[i];
                if (toggle is not null)
                    list.Add(toggle);
            }

            if (list.Count > 0)
                return list;
        }

        var viewport = carouselParent?.parent;
        if (viewport is null)
            return new List<M1Toggle>();

        var toggles = viewport.GetComponentsInChildren<M1Toggle>(true);
        var fallback = new List<M1Toggle>(toggles.Length);
        for (int i = 0; i < toggles.Length; i++)
        {
            var toggle = toggles[i];
            if (toggle is not null)
                fallback.Add(toggle);
        }

        return fallback;
    }

    /// <summary>
    /// Computes slot width and step from the available viewport width and tab spacing.
    /// </summary>
    public void SetMetrics(float viewportWidth, float tabSpacing)
    {
        slotHeight = style.Height > 0 ? style.Height : 60f;
        var baseWidth = style.Width > 0 ? style.Width : 220f;

        var fitWidth = viewportWidth > 0
            ? (viewportWidth - tabSpacing * (TabCarouselLayout.VisibleSlotCount - 1)) / TabCarouselLayout.VisibleSlotCount
            : baseWidth;

        slotWidth = Mathf.Min(baseWidth, fitWidth);
        slotWidth = Mathf.Max(slotWidth, 20f);

        step = slotWidth + tabSpacing;
    }

    /// <summary>
    /// Rebuilds the carousel slots to match the current <see cref="M1ToggleGroup"/>.
    /// </summary>
    public void Rebuild(M1ToggleGroup? group)
    {
        var newRing = CollectToggles(group);
        var newLabels = new List<string>(newRing.Count);
        foreach (var toggle in newRing)
            newLabels.Add(GetLabel(toggle));

        var sizeUnchanged = slots.Count > 0 && Mathf.Abs(slots[0].RectTransform.sizeDelta.x - slotWidth) < 0.01f;
        if (newRing.Count == ring.Count && ring.SequenceEqual(newRing) && sizeUnchanged)
        {
            for (int i = 0; i < newLabels.Count; i++)
                labels[i] = newLabels[i];
            return;
        }

        ring.Clear();
        ring.AddRange(newRing);

        labels.Clear();
        labels.AddRange(newLabels);

        ClearSlots();

        if (ring.Count == 0)
            return;

        var half = (TabCarouselLayout.SlotCount - 1) / 2;
        for (int i = 0; i < TabCarouselLayout.SlotCount; i++)
        {
            var offset = i - half;
            var x = offset * step;
            var slot = TabCarouselSlotBuilder.Build(
                carouselParent,
                style,
                slotWidth,
                slotHeight,
                new Vector2(x, 0f));

            slots.Add(slot);
            var slotIndex = i;
            slot.SetOnClick(() => onTabSelected(TabCarouselLayout.Mod(slots[slotIndex].ContentIndex, ring.Count)));
        }

        if (targetMod < 0)
        {
            currentActive = 0f;
            targetActive = 0f;
            targetMod = 0;
        }

        ResetSlotContents();
    }

    /// <summary>
    /// Starts animating so the active tab index matches the given toggle.
    /// </summary>
    public void SetActive(M1Toggle? activeToggle)
    {
        if (ring.Count == 0 || activeToggle is null)
            return;

        var index = ring.IndexOf(activeToggle);
        if (index < 0)
            return;

        if (index == targetMod)
            return;

        var delta = TabCarouselLayout.ShortestDelta(currentActive, index, ring.Count);
        targetActive = currentActive + delta;
        targetMod = index;
        isTransitioning = true;
    }

    /// <summary>
    /// Updates animation, recycles off-screen slots, and refreshes visuals.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (ring.Count == 0)
            return;

        if (isTransitioning)
        {
            var speed = 1f / TransitionDuration;
            var maxDelta = speed * deltaTime;
            var diff = targetActive - currentActive;

            if (Mathf.Abs(diff) <= maxDelta)
            {
                currentActive = targetActive;
                isTransitioning = false;
            }
            else
            {
                currentActive += Mathf.Sign(diff) * maxDelta;
            }
        }

        var activeInt = Mathf.RoundToInt(currentActive);
        var visibleSlots = new List<TabCarouselSlot>();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var visualPosition = TabCarouselLayout.GetVisualPosition(slot.ContentIndex, currentActive, step);

            if (TabCarouselLayout.IsOffScreen(visualPosition, step))
            {
                var fromLeft = visualPosition < 0f;
                var newContentIndex = fromLeft
                    ? TabCarouselLayout.GetRecycledRightContentIndex(currentActive)
                    : TabCarouselLayout.GetRecycledLeftContentIndex(currentActive);

                slot.SetContent(newContentIndex, labels[TabCarouselLayout.Mod(newContentIndex, ring.Count)]);
                visualPosition = TabCarouselLayout.GetVisualPosition(slot.ContentIndex, currentActive, step);
            }

            var distance = MathF.Abs(slot.ContentIndex - currentActive);
            var (alpha, scale) = TabCarouselLayout.GetVisualState(distance);
            var isSelected = slot.ContentIndex == activeInt;
            var interactable = alpha > 0.25f;

            slot.UpdateVisual(visualPosition, alpha, scale, isSelected, interactable);

            if (interactable)
                visibleSlots.Add(slot);
        }

        UpdateNavigation(visibleSlots);
    }

    private void ResetSlotContents()
    {
        var half = (TabCarouselLayout.SlotCount - 1) / 2;

        for (int i = 0; i < slots.Count; i++)
        {
            var offset = i - half;
            var contentIndex = TabCarouselLayout.GetDesiredContentIndex(currentActive, offset);
            slots[i].SetContent(contentIndex, labels[TabCarouselLayout.Mod(contentIndex, ring.Count)]);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot.RectTransform != null)
                UnityEngine.Object.Destroy(slot.RectTransform.gameObject);
        }

        slots.Clear();
    }

    private static string GetLabel(M1Toggle toggle)
    {
        var typeName = toggle.transform.Find("type_name")?.GetComponent<TextMeshProUGUI>();
        return typeName?.text ?? toggle.name ?? string.Empty;
    }

    private static void UpdateNavigation(List<TabCarouselSlot> visibleSlots)
    {
        if (visibleSlots.Count == 0)
            return;

        visibleSlots.Sort((a, b) => a.ContentIndex.CompareTo(b.ContentIndex));

        for (int i = 0; i < visibleSlots.Count; i++)
        {
            var left = visibleSlots[(i - 1 + visibleSlots.Count) % visibleSlots.Count].Button;
            var right = visibleSlots[(i + 1) % visibleSlots.Count].Button;

            visibleSlots[i].Button.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = left,
                selectOnRight = right,
            };
        }
    }
}
