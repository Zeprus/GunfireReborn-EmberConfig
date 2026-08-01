namespace EmberConfig.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

public class RowHoverHandler : MonoBehaviour
{
    public RowHoverHandler(IntPtr ptr) : base(ptr) { }

    public Image? Background;
    public Color NormalColor;
    public Color HighlightColor;

    public bool IsHovered { get; private set; }

    private RectTransform? rectTransform;
    private Canvas? parentCanvas;
    private Camera? canvasCamera;

    private void OnEnable()
    {
        rectTransform = transform as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
        canvasCamera = parentCanvas?.worldCamera;
    }

    private void OnDisable()
    {
        SetHovered(false);
    }

    public void UpdateHover()
    {
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (rectTransform == null || rectTransform.Pointer == IntPtr.Zero)
            return;

        if (Background != null && Background.Pointer == IntPtr.Zero)
            Background = null;

        bool hovered = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, (Vector2)Input.mousePosition, canvasCamera);
        if (hovered != IsHovered)
            SetHovered(hovered);
    }

    private void SetHovered(bool hovered)
    {
        IsHovered = hovered;
        if (hovered)
            ApplyHover();
        else
            ApplyNormal();
    }

    private void ApplyHover()
    {
        if (Background == null || Background.Pointer == IntPtr.Zero)
            return;

        Background.color = HighlightColor;
    }

    private void ApplyNormal()
    {
        if (Background == null || Background.Pointer == IntPtr.Zero)
            return;

        Background.color = NormalColor;
    }
}
