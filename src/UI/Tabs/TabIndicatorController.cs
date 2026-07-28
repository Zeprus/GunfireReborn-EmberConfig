namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Page-dot indicator under the tab carousel. Each dot represents one tab;
/// color and scale are driven by circular distance from the active tab.
/// </summary>
internal sealed class TabIndicatorController
{
    private readonly RectTransform indicatorParent;
    private readonly TabStyle style;
    private readonly List<(RectTransform Rect, Image Image)> dots = new();

    private int lastCount = -1;
    private static Sprite? circleSprite;

    public TabIndicatorController(RectTransform indicatorParent, TabStyle style)
    {
        this.indicatorParent = indicatorParent ?? throw new ArgumentNullException(nameof(indicatorParent));
        this.style = style;
    }

    public void Rebuild(int count)
    {
        if (count == lastCount && dots.Count == count)
            return;

        lastCount = count;
        ClearDots();

        if (count <= 0)
            return;

        var sprite = GetCircleSprite();
        var width = indicatorParent.sizeDelta.x > 0 ? indicatorParent.sizeDelta.x : indicatorParent.rect.width;
        var dotSize = Mathf.Min(10f, width / Mathf.Max(1, count * 2f));
        var spacing = width / Mathf.Max(1, count);
        var half = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Dot_{i}");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(indicatorParent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(dotSize, dotSize);
            rect.anchoredPosition = new Vector2((i - half) * spacing, 0f);

            go.AddComponent<CanvasRenderer>();
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = style.Unselected.Color;
            image.raycastTarget = false;

            dots.Add((rect, image));
        }
    }

    public void Update(float currentActive, int count)
    {
        if (count != lastCount)
            Rebuild(count);

        if (dots.Count == 0)
            return;

        var selectedColor = style.Selected.Color;
        var unselectedColor = style.Unselected.Color;
        var length = count;
        var halfLength = length / 2f;

        for (int i = 0; i < dots.Count; i++)
        {
            var raw = (i - currentActive) % length;
            if (raw < 0f)
                raw += length;
            if (raw > halfLength)
                raw -= length;

            var distance = MathF.Abs(raw);
            var t = Mathf.Clamp(distance / halfLength, 0f, 1f);

            var color = Color.Lerp(selectedColor, unselectedColor, t);
            color.a = Mathf.Lerp(1f, 0.4f, t);

            dots[i].Image.color = color;
            dots[i].Rect.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t);
        }
    }

    private void ClearDots()
    {
        foreach (var dot in dots)
        {
            if (dot.Rect != null)
                UnityEngine.Object.Destroy(dot.Rect.gameObject);
        }

        dots.Clear();
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int Size = 16;
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var center = new Vector2((Size - 1) / 2f, (Size - 1) / 2f);
        var radius = (Size - 1) / 2f;
        var pixels = new Color[Size * Size];

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                var dist = Vector2.Distance(center, new Vector2(x, y));
                var index = y * Size + x;
                pixels[index] = dist <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, Size, Size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);

        return circleSprite;
    }
}
