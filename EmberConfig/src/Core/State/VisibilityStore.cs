namespace EmberConfig.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using EmberConfig.Public;

/// <summary>
/// Stores per-mod/tab visibility toggles in EmberConfig's own BepInEx config
/// and creates the corresponding setting rows in the EmberConfig tab.
/// </summary>
public sealed class VisibilityStore
{
    public const string VisibilityTab = "EmberConfig";
    public const string VisibilityGroup = "Settings Visibility";
    public const string VisibilitySection = "Visibility";
    public const string SentinelModName = "__EmberConfigVisibility";

    private static VisibilityStore? current;

    public static VisibilityStore Current
    {
        get => current ?? throw new InvalidOperationException("VisibilityStore has not been initialized. Ensure EmberConfig.Plugin.Load has run.");
        set => current = value;
    }

    public static bool IsInitialized => current is not null;

    private readonly ConfigFile configFile;
    private readonly Dictionary<string, ConfigEntry<bool>> entries = new(StringComparer.Ordinal);
    private readonly HashSet<string> registeredSwitches = new(StringComparer.Ordinal);

    public VisibilityStore(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));
    }

    public static void Initialize(ConfigFile configFile)
    {
        current = new VisibilityStore(configFile);
    }

    /// <summary>
    /// Returns whether the given mod's rows should be visible in the given tab.
    /// Always returns true for EmberConfig's own settings and for visibility rows.
    /// </summary>
    public bool IsVisible(string modName, string tabName)
    {
        if (IsAlwaysVisible(modName, tabName))
            return true;

        var key = GetKey(modName, tabName);
        if (entries.TryGetValue(key, out var entry))
            return entry.Value;

        return true;
    }

    /// <summary>
    /// Gets or creates a <see cref="ConfigEntry{T}" /> for a mod/tab visibility toggle.
    /// </summary>
    public ConfigEntry<bool> GetOrCreate(string modName, string tabName)
    {
        var key = GetKey(modName, tabName);
        if (entries.TryGetValue(key, out var existing))
            return existing;

        var configKey = SanitizeConfigKey($"{modName} :: {tabName}");
        var description = new ConfigDescription($"Show settings for '{modName}' in the '{tabName}' tab.", null);
        var entry = configFile.Bind(VisibilitySection, configKey, true, description);
        entries[key] = entry;
        return entry;
    }

    /// <summary>
    /// Ensures a visibility switch exists for the consumer entry's (ModName, Tab).
    /// The switch is registered in the EmberConfig tab under the "Settings Visibility" group,
    /// with the mod name as the sub-group.
    /// </summary>
    public void EnsureVisibilitySwitch(ISettingEntry consumerEntry)
    {
        if (consumerEntry is null)
            throw new ArgumentNullException(nameof(consumerEntry));

        var modName = consumerEntry.ModName;
        var tabName = consumerEntry.Location.Tab;

        if (IsAlwaysVisible(modName, tabName))
            return;

        var key = GetKey(modName, tabName);
        if (!registeredSwitches.Add(key))
            return;

        var config = GetOrCreate(modName, tabName);
        var entry = new SettingEntry<bool>(
            Guid.NewGuid().ToString("N"),
            config,
            tabName,
            SentinelModName,
            new SettingLocation(VisibilityTab, VisibilityGroup, modName),
            _ => SettingsMenuManager.Current?.RequestVisibilityRefresh(modName, tabName),
            SettingControlStyle.Switch,
            null);

        SettingsRegistry.Current.Register(entry);
    }

    /// <summary>
    /// Resets all visibility toggles by deleting them from the config and re-creating
    /// switches for every registered consumer. This restores default (visible) state
    /// for all current and previously registered mods.
    /// </summary>
    public void ResetAllVisibility()
    {
        if (!SettingsRegistry.IsInitialized)
            return;

        // Remove visibility config entries from the BepInEx config file, including
        // switches for mods that are no longer loaded.
        foreach (var key in configFile
            .Where(kvp => string.Equals(kvp.Key.Section, VisibilitySection, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList())
            configFile.Remove(key);

        // Unregister the old visibility rows so they do not reference removed config entries.
        foreach (var row in SettingsRegistry.Current.Entries.OfType<ISettingEntry>().Where(e => e.ModName == SentinelModName).ToList())
            SettingsRegistry.Current.Unregister(row);

        entries.Clear();
        registeredSwitches.Clear();

        configFile.Save();

        // Re-create visibility switches for all current consumer entries.
        foreach (var consumer in SettingsRegistry.Current.Entries.Where(e => e.ModName != SentinelModName && !IsAlwaysVisible(e.ModName, e.Location.Tab)).ToList())
            EnsureVisibilitySwitch(consumer);
    }

    private static bool IsAlwaysVisible(string modName, string tabName)
    {
        if (modName == SentinelModName)
            return true;

        if (modName == "EmberConfig" && tabName == VisibilityTab)
            return true;

        return false;
    }

    private static string GetKey(string modName, string tabName)
    {
        return $"{modName.Trim().ToLowerInvariant()}\x1F{tabName.Trim().ToLowerInvariant()}";
    }

    private static string SanitizeConfigKey(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (c == '=' || c == ':' || c == ';' || c == '#' || c == '[' || c == ']' || char.IsControl(c))
                sb.Append('_');
            else
                sb.Append(c);
        }

        return sb.ToString().Trim();
    }
}
