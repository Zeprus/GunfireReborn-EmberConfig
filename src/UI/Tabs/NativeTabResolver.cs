namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maps native tab names to their vanilla content panels and scans the vanilla
/// tab_switch bar for native <see cref="M1Toggle"/> instances.
/// </summary>
internal sealed class NativeTabResolver
{
    private static readonly Dictionary<string, string> NativeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Game Settings", "Content_1" },
        { "Mouse/Keyboard", "Content_2" },
        { "Video", "Content_3" },
        { "Audio", "Content_4" },
        { "Controller", "Content_5" }
    };

    private readonly List<M1Toggle> nativeToggles = new();
    private readonly Dictionary<M1Toggle, string> toggleToContentName = new();

    public IReadOnlyList<M1Toggle> NativeToggles => nativeToggles;

    public bool TryGetContentName(M1Toggle toggle, [NotNullWhen(true)] out string? contentName)
    {
        if (toggleToContentName.TryGetValue(toggle, out var value))
        {
            contentName = value;
            return true;
        }

        contentName = null;
        return false;
    }

    public void Scan(Transform tabSwitch)
    {
        nativeToggles.Clear();
        toggleToContentName.Clear();

        if (tabSwitch is null)
            return;

        for (int i = 0; i < tabSwitch.childCount; i++)
        {
            var child = tabSwitch.GetChild(i);
            if (child is null)
                continue;

            var toggle = child.GetComponent<M1Toggle>();
            if (toggle is null)
                continue;

            if (child.name.StartsWith("tab_custom_", StringComparison.OrdinalIgnoreCase))
                continue;

            nativeToggles.Add(toggle);
            toggleToContentName[toggle] = ResolveContentName(child);
        }
    }

    public void Clear()
    {
        nativeToggles.Clear();
        toggleToContentName.Clear();
    }

    public static bool IsNativeTab(string tabName) =>
        NativeMappings.ContainsKey(Normalize(tabName));

    public static bool TryGetNativeContentName(string tabName, [NotNullWhen(true)] out string? contentName)
    {
        if (NativeMappings.TryGetValue(Normalize(tabName), out var value))
        {
            contentName = value;
            return true;
        }

        contentName = null;
        return false;
    }

    public static IEnumerable<string> GetNativeContentNames() =>
        NativeMappings.Values.Distinct(StringComparer.OrdinalIgnoreCase);

    public static string ResolveContentName(Transform tabRoot)
    {
        var typeName = tabRoot.Find("type_name")?.GetComponent<TextMeshProUGUI>();
        var background = tabRoot.Find("Background")?.GetComponent<TextMeshProUGUI>();
        var text = typeName?.text ?? background?.text ?? tabRoot.name ?? string.Empty;

        if (NativeMappings.TryGetValue(text, out var mapped))
            return mapped;

        if (int.TryParse(tabRoot.name, out var index) && index >= 1 && index <= NativeMappings.Count)
            return $"Content_{index}";

        return "Content_1";
    }

    private static string Normalize(string name) => name?.Trim() ?? string.Empty;
}
