namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct RowStyle(
    TextAppearance Title,
    Sprite? BackgroundSprite,
    Color BackgroundColor,
    Color HighlightColor,
    Image.Type BackgroundType,
    float Height,
    float Width,
    float TitleWidth,
    float ItemWidth,
    TextMeshProUGUI? DescriptionText,
    RectData RowRect,
    RectData TitleRect,
    RectData ItemRect)
{
    internal static class Layout
    {
        internal const float DefaultHeight = 50f;
        internal const float DefaultWidth = 1000f;
        internal const float DefaultTitleWidth = 474.8f;
        internal const float DefaultItemWidth = 473.3f;
    }

    internal static RowStyle? Capture(Transform panelRoot)
    {
        var rowRoot = PickBestRow(FindRowCandidates(panelRoot));
        if (rowRoot is null)
            return null;

        var titleTransform = rowRoot.Find("Title");
        var titleText = titleTransform?.GetComponent<TextMeshProUGUI>();
        if (titleText is null)
            return null;

        var itemTransform = rowRoot.Find("Item");

        var rowImage = rowRoot.GetComponent<Image>();
        var backgroundSprite = rowImage?.sprite ?? FindRowBackgroundSprite(panelRoot);
        var backgroundType = rowImage?.type ?? Image.Type.Sliced;

        var selectable = rowRoot.GetComponent<Selectable>();
        var colorMultiplier = selectable?.colors.colorMultiplier ?? 1f;
        var backgroundColor = selectable is not null
            ? selectable.colors.normalColor * colorMultiplier
            : (rowImage?.canvasRenderer?.GetColor() ?? new Color(0.1f, 0.1f, 0.1f, 1f));
        var highlightColor = selectable is not null
            ? selectable.colors.highlightedColor * colorMultiplier
            : new Color(0.22f, 0.22f, 0.22f, 1f);

        var descriptionText = panelRoot.Find("bg_windows/setting_desc/desc")?.GetComponent<TextMeshProUGUI>();

        var rowRectTransform = rowRoot.GetComponent<RectTransform>();
        var titleRectTransform = titleTransform?.GetComponent<RectTransform>();
        var itemRectTransform = itemTransform?.GetComponent<RectTransform>();

        var rowRectData = rowRectTransform is not null ? RectData.From(rowRectTransform) : DefaultRowRect;
        var titleRectData = titleRectTransform is not null ? RectData.From(titleRectTransform) : DefaultTitleRect;
        var itemRectData = itemRectTransform is not null ? RectData.From(itemRectTransform) : DefaultItemRect;

        var width = rowRectData.SizeDelta.x > 0f ? rowRectData.SizeDelta.x : Layout.DefaultWidth;
        var height = rowRectData.SizeDelta.y > 0f ? rowRectData.SizeDelta.y : Layout.DefaultHeight;
        var titleWidth = titleRectData.SizeDelta.x > 0f ? titleRectData.SizeDelta.x : Layout.DefaultTitleWidth;
        var itemWidth = itemRectData.SizeDelta.x > 0f ? itemRectData.SizeDelta.x : Layout.DefaultItemWidth;

        return new RowStyle(
            TextAppearance.From(titleText, 20f),
            backgroundSprite,
            backgroundColor,
            highlightColor,
            backgroundType,
            height,
            width,
            titleWidth,
            itemWidth,
            descriptionText,
            rowRectData,
            titleRectData,
            itemRectData);
    }

    private static readonly RectData DefaultRowRect = new(
        new Vector2(0f, 1f), new Vector2(0f, 1f),
        new Vector2(Layout.DefaultWidth, Layout.DefaultHeight),
        new Vector2(Layout.DefaultWidth / 2f, -Layout.DefaultHeight / 2f),
        new Vector2(0.5f, 0.5f));

    private static readonly RectData DefaultTitleRect = new(
        new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
        new Vector2(Layout.DefaultTitleWidth, Layout.DefaultHeight),
        new Vector2(25f, 0f),
        new Vector2(0f, 0.5f));

    private static readonly RectData DefaultItemRect = new(
        new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
        new Vector2(Layout.DefaultItemWidth, Layout.DefaultHeight),
        Vector2.zero,
        new Vector2(1f, 0.5f));

    private static List<Transform> FindRowCandidates(Transform root)
    {
        var candidates = new List<Transform>();
        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            var t = texts[i];
            if (!string.Equals(t.gameObject.name, "Title", StringComparison.OrdinalIgnoreCase))
                continue;

            var rowRoot = t.transform.parent;
            if (rowRoot is null || rowRoot.GetComponent<RectTransform>() is null)
                continue;

            if (rowRoot.Find("Item") is not null)
                candidates.Add(rowRoot);
        }

        return candidates;
    }

    private static Transform? PickBestRow(List<Transform> candidates)
    {
        if (candidates.Count == 0)
            return null;

        Transform? best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            var rect = candidate.GetComponent<RectTransform>();
            if (rect is null)
                continue;

            var item = candidate.Find("Item");
            var itemRect = item?.GetComponent<RectTransform>();

            float height = rect.sizeDelta.y;
            float itemHeight = itemRect?.sizeDelta.y ?? height;
            bool active = candidate.gameObject.activeInHierarchy;
            bool hasSlider = item?.Find("Slider_PCunit") is not null;

            // Prefer rows that look like a normal setting row:
            // active in hierarchy, ~50 px tall, item ~50 px tall, and slider rows get a bonus
            // because their item width is the widest and safest for most controls.
            float score = Mathf.Abs(height - Layout.DefaultHeight) * 2f
                        + Mathf.Abs(itemHeight - Layout.DefaultHeight) * 2f
                        + (active ? 0f : 500f)
                        + (hasSlider ? -100f : 0f);

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best ?? candidates[0];
    }

    private static Sprite? FindRowBackgroundSprite(Transform root)
    {
        var images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img.sprite is not null && string.Equals(img.sprite.name, "Background", StringComparison.OrdinalIgnoreCase))
                return img.sprite;
        }

        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img.sprite is not null)
                return img.sprite;
        }

        return null;
    }
}
