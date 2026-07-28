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

    public event Action<ISettingEntry>? EntryRegistered;

    private readonly List<ISettingEntry> entries = new();
    private readonly Dictionary<string, List<ISettingEntry>> byTab = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string Tab, string Group), List<ISettingEntry>> byGroup = new(GroupKeyComparer.Instance);

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

        var groupKey = (tab, Normalize(entry.Location.Group));
        if (!byGroup.TryGetValue(groupKey, out var groupEntries))
        {
            groupEntries = new List<ISettingEntry>();
            byGroup[groupKey] = groupEntries;
        }
        groupEntries.Add(entry);
        EntryRegistered?.Invoke(entry);
    }

    public IEnumerable<ISettingEntry> GetAll() => entries;

    public IEnumerable<ISettingEntry> GetByTab(string tab)
    {
        return byTab.TryGetValue(Normalize(tab), out var tabEntries) ? tabEntries : Enumerable.Empty<ISettingEntry>();
    }

    public IEnumerable<ISettingEntry> GetByGroup(string tab, string group)
    {
        var key = (Normalize(tab), Normalize(group));
        return byGroup.TryGetValue(key, out var groupEntries) ? groupEntries : Enumerable.Empty<ISettingEntry>();
    }

    public IEnumerable<IKeybindEntry> GetKeybindEntries() => entries.OfType<KeybindEntry>();

    public IEnumerable<string> GetTabs() => byTab.Keys;

    private static string Normalize(string value) => value?.Trim() ?? string.Empty;

    private sealed class GroupKeyComparer : IEqualityComparer<(string Tab, string Group)>
    {
        public static readonly GroupKeyComparer Instance = new();

        public bool Equals((string Tab, string Group) x, (string Tab, string Group) y)
            => string.Equals(x.Tab, y.Tab, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.Group, y.Group, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Tab, string Group) obj)
            => HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Tab),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Group));
    }
}
