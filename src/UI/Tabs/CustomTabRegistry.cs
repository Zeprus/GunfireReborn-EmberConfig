namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal sealed class CustomTabRegistry
{
    private readonly Dictionary<string, CustomTab?> tabs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<M1Toggle, string> toggleToName = new();

    public IEnumerable<CustomTab> All => tabs.Values.OfType<CustomTab>();

    public bool TryGet(string tabName, [NotNullWhen(true)] out CustomTab? tab) => tabs.TryGetValue(tabName, out tab);

    public bool TryGetName(M1Toggle toggle, [NotNullWhen(true)] out string? tabName)
    {
        if (toggleToName.TryGetValue(toggle, out var value))
        {
            tabName = value;
            return true;
        }

        tabName = null;
        return false;
    }

    public void Register(string tabName, CustomTab tab)
    {
        if (tab is null)
            throw new ArgumentNullException(nameof(tab));

        tabs[tabName] = tab;
        toggleToName[tab.Toggle] = tabName;
    }

    public bool Unregister(string tabName)
    {
        if (!tabs.TryGetValue(tabName, out var tab) || tab is null)
            return false;

        tabs.Remove(tabName);
        toggleToName.Remove(tab.Toggle);
        return true;
    }

    public void DestroyAll(M1ToggleGroup? toggleGroup)
    {
        foreach (var tab in All.ToList())
        {
            if (toggleGroup is not null)
                toggleGroup.UnregisterToggle(tab.Toggle);

            UnityEngine.Object.Destroy(tab.Toggle.gameObject);
            UnityEngine.Object.Destroy(tab.Content.gameObject);
        }

        tabs.Clear();
        toggleToName.Clear();
    }
}
