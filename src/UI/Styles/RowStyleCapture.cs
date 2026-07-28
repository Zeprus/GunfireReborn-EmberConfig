namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class RowStyleCapture
{
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

        var rowRectData = rowRectTransform is not null ? RectData.From(rowRectTransform) : RowStyle.DefaultRowRect;
        var titleRectData = titleRectTransform is not null ? RectData.From(titleRectTransform) : RowStyle.DefaultTitleRect;
        var itemRectData = itemRectTransform is not null ? RectData.From(itemRectTransform) : RowStyle.DefaultItemRect;

        var width = rowRectData.SizeDelta.x > 0f ? rowRectData.SizeDelta.x : RowStyle.Layout.DefaultWidth;
        var height = rowRectData.SizeDelta.y > 0f ? rowRectData.SizeDelta.y : RowStyle.Layout.DefaultHeight;
        var titleWidth = titleRectData.SizeDelta.x > 0f ? titleRectData.SizeDelta.x : RowStyle.Layout.DefaultTitleWidth;
        var itemWidth = itemRectData.SizeDelta.x > 0f ? itemRectData.SizeDelta.x : RowStyle.Layout.DefaultItemWidth;

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

            float score = Mathf.Abs(height - RowStyle.Layout.DefaultHeight) * 2f
                        + Mathf.Abs(itemHeight - RowStyle.Layout.DefaultHeight) * 2f
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
