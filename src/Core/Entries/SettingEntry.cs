namespace SettingsLib.Core;

using System;
using BepInEx.Configuration;

public sealed class SettingEntry<T> : ISettingEntry
{
    public string Id { get; }
    public ConfigEntry<T> Config { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public Action<T>? OnValueChanged { get; }
    public event Action? ValueChanged;

    ConfigEntryBase ISettingEntry.Config => Config;

    public SettingEntry(string id, ConfigEntry<T> config, string label, SettingLocation location, Action<T>? onValueChanged = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));

        Id = id;
        Config = config ?? throw new ArgumentNullException(nameof(config));
        Label = label;
        Location = location;
        OnValueChanged = onValueChanged;

        Config.SettingChanged += (_, _) =>
        {
            OnValueChanged?.Invoke(Config.Value);
            ValueChanged?.Invoke();
        };
    }
}
