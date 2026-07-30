namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using EmberConfig.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIFinder
{
    private Transform? panelRoot;
    private Transform? viewport;
    private Transform? tabSwitch;
    private ScrollRect? scrollRect;
    private readonly Dictionary<string, Transform> contentPanels = new(StringComparer.OrdinalIgnoreCase);

    public bool IsReady { get; private set; }
    internal StyleCatalog? Style { get; private set; }
    public Transform? Viewport => viewport;
    public Transform? TabSwitch => tabSwitch;
    public ScrollRect? ScrollRect => scrollRect;

    public void Initialize(Transform panelRoot)
    {
        this.panelRoot = panelRoot;
        contentPanels.Clear();

        var bgWindows = TransformFinder.Find(panelRoot, "bg_windows");
        var scroll = TransformFinder.Find(bgWindows, "setting_scroll");
        viewport = TransformFinder.Find(scroll, "Viewport");
        tabSwitch = TransformFinder.Find(bgWindows, "tab_switch");
        scrollRect = scroll?.GetComponent<ScrollRect>();

        if (viewport is not null)
        {
            for (int i = 0; i < viewport.childCount; i++)
            {
                var c = viewport.GetChild(i);
                if (c.name.StartsWith("Content_", StringComparison.OrdinalIgnoreCase))
                    contentPanels[c.name] = c;
            }
        }

        Style = StyleCatalog.Create(panelRoot);
        IsReady = Style is not null && viewport is not null && tabSwitch is not null;
    }

    public void Reset()
    {
        panelRoot = null;
        viewport = null;
        tabSwitch = null;
        scrollRect = null;
        contentPanels.Clear();
        Style = null;
        IsReady = false;
    }

    public Transform? GetContent(string name) =>
        contentPanels.TryGetValue(name, out var t) ? t : null;

}
