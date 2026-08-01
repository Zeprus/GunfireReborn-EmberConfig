namespace EmberConfig.Core;

using System;
using BepInEx.Configuration;
using EmberConfig.Public;

public sealed class SettingEntry<T> : ISettingEntry
{
    public string Id { get; }
    public ConfigEntry<T> Config { get; }
    public string Label { get; }
    public string ModName { get; }
    public SettingLocation Location { get; }
    public Action<T>? OnValueChanged { get; }
    public SettingControlStyle ControlStyle { get; }
    public SwitchLabels? SwitchLabels { get; }
    public event Action? ValueChanged;

    ConfigEntryBase ISettingEntry.Config => Config;

    public SettingEntry(
        string id,
        ConfigEntry<T> config,
        string label,
        string modName,
        SettingLocation location,
        Action<T>? onValueChanged = null,
        SettingControlStyle controlStyle = SettingControlStyle.Auto,
        SwitchLabels? switchLabels = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));
        if (string.IsNullOrWhiteSpace(modName))
            throw new ArgumentException("ModName cannot be empty.", nameof(modName));

        Id = id;
        Config = config ?? throw new ArgumentNullException(nameof(config));
        Label = label;
        ModName = modName;
        Location = location;
        OnValueChanged = onValueChanged;
        ControlStyle = controlStyle;
        SwitchLabels = switchLabels;

        Config.SettingChanged += (_, _) =>
        {
            OnValueChanged?.Invoke(Config.Value);
            ValueChanged?.Invoke();
        };
    }
}
