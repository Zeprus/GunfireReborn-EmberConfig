namespace EmberConfig.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SettingsRegistry
{
    private static SettingsRegistry? current;

    public static SettingsRegistry Current
    {
        get => current ?? throw new InvalidOperationException("SettingsRegistry.Current has not been initialized. Ensure EmberConfig.Plugin.Load has run.");
        set => current = value;
    }

    public static bool IsInitialized => current is not null;

    public event Action? EntryRegistered;

    private readonly List<ISettingEntry> entries = new();
    private readonly Dictionary<string, List<ISettingEntry>> byTab = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ISettingEntry> Entries => entries;

    public void Register(ISettingEntry entry)
    {
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        entries.Add(entry);

        var tab = Normalize(entry.Location.Tab);
        if (!byTab.TryGetValue(tab, out var tabEntries))
        {
            tabEntries = new List<ISettingEntry>();
            byTab[tab] = tabEntries;
        }
        tabEntries.Add(entry);

        EntryRegistered?.Invoke();
    }

    public IEnumerable<ISettingEntry> GetByTab(string tab)
    {
        return byTab.TryGetValue(Normalize(tab), out var tabEntries) ? tabEntries : Enumerable.Empty<ISettingEntry>();
    }

    public IEnumerable<IKeybindEntry> GetKeybindEntries() => entries.OfType<KeybindEntry>();

    public IEnumerable<string> GetTabs() => byTab.Keys;

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
}
