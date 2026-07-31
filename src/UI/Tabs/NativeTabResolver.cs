namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maps native tab names to their vanilla content panels and provides
/// enough data for <see cref="TabBarController"/> to rebuild fresh
/// native <see cref="M1Toggle"/> buttons for a scrollable tab bar.
/// </summary>
internal sealed class NativeTabResolver
{
    private static readonly IReadOnlyList<string> NativeTabNames = new List<string>
    {
        "Game Settings",
        "Mouse/Keyboard",
        "Video",
        "Audio",
        "Controller"
    };

    private static readonly Dictionary<string, string> NativeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Game Settings", "Content_1" },
        { "Mouse/Keyboard", "Content_2" },
        { "Video", "Content_3" },
        { "Audio", "Content_4" },
        { "Controller", "Content_5" }
    };

    private static readonly Dictionary<string, string> ContentToName =
        NativeMappings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

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

    /// <summary>
    /// Scans the original vanilla tab_switch transform and returns the
    /// data needed to recreate native tabs in a scrollable list.
    /// </summary>
    public IReadOnlyList<NativeTabInfo> Scan(Transform tabSwitch)
    {
        var infos = new List<NativeTabInfo>();

        if (tabSwitch is null)
            return infos;

        for (int i = 0; i < tabSwitch.childCount; i++)
        {
            var child = tabSwitch.GetChild(i);
            if (child is null)
                continue;

            if (child.name.StartsWith("tab_custom_", StringComparison.OrdinalIgnoreCase))
                continue;

            var toggle = child.GetComponent<M1Toggle>();
            if (toggle is null)
                continue;

            var label = child.Find("type_name")?.GetComponent<TextMeshProUGUI>()?.text ?? child.name;
            var contentName = ResolveContentName(child);

            infos.Add(new NativeTabInfo(label, contentName, toggle.isOn));
        }

        return infos;
    }

    public void Register(M1Toggle toggle, string contentName)
    {
        if (toggle is null)
            throw new ArgumentNullException(nameof(toggle));
        if (string.IsNullOrEmpty(contentName))
            throw new ArgumentException("Content name cannot be null or empty.", nameof(contentName));

        if (!toggleToContentName.ContainsKey(toggle))
            nativeToggles.Add(toggle);

        toggleToContentName[toggle] = contentName;
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

    public static bool TryGetNativeTabName(string contentName, [NotNullWhen(true)] out string? tabName)
    {
        if (ContentToName.TryGetValue(contentName, out var value))
        {
            tabName = value;
            return true;
        }

        tabName = null;
        return false;
    }

    public static int GetNativeTabOrder(string tabName)
    {
        var normalized = Normalize(tabName);
        for (int i = 0; i < NativeTabNames.Count; i++)
        {
            if (string.Equals(NativeTabNames[i], normalized, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return int.MaxValue;
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

    internal readonly record struct NativeTabInfo(string Label, string ContentName, bool IsActive);
}
