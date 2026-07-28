namespace SettingsLib.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates custom tab content panels and M1Toggle buttons.
/// </summary>
internal sealed class CustomTabFactory
{
    private readonly UIFinder uiFinder;

    public CustomTabFactory(UIFinder uiFinder)
    {
        this.uiFinder = uiFinder ?? throw new ArgumentNullException(nameof(uiFinder));
    }

    public CustomTab? Create(string tabName, RectTransform tabContent, TabStyle? tabStyle)
    {
        if (uiFinder.Viewport is null || uiFinder.Style is null || tabContent is null)
        {
            Plugin.Logger?.LogWarning($"SettingsLib: cannot create custom tab '{tabName}' because the UI is not ready.");
            return null;
        }

        var content = CreateContent(tabName);
        var style = tabStyle ?? TabStyle.Fallback(uiFinder.Style.Row.Title);
        var toggle = TabButtonBuilder.Build($"tab_custom_{tabName}", tabName, style, tabContent);

        return new CustomTab(toggle.transform, content, toggle);
    }

    private Transform CreateContent(string tabName)
    {
        var viewport = uiFinder.Viewport!;
        var go = new GameObject($"Content_SL_{tabName}");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(viewport, false);

        var coverMask = viewport.Find("covery_mask");
        if (coverMask != null)
            rect.SetSiblingIndex(coverMask.GetSiblingIndex());

        RowElementBuilder.SetRect(rect, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        rect.pivot = new Vector2(0.5f, 1f);

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 0f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        go.SetActive(false);
        return go.transform;
    }
}
