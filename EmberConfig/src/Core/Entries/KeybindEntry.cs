namespace EmberConfig.Core;

using System;
using BepInEx.Configuration;
using EmberConfig.Public;
using UnityEngine;

public sealed class KeybindEntry : ISettingEntry, IKeybindEntry
{
    public string Id { get; }
    public ConfigEntry<KeyCode> Primary { get; }
    public ConfigEntry<KeyCode>? Secondary { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public Action? OnPressed { get; }
    public Action? OnReleased { get; }
    public event Action? ValueChanged;

    public int PrimaryKeyCodeValue => (int)Primary.Value;

    public int? SecondaryKeyCodeValue =>
        Secondary is { Value: not KeyCode.None } secondary ? (int)secondary.Value : null;

    public SettingControlStyle ControlStyle { get; } = SettingControlStyle.Auto;

    public SwitchLabels? SwitchLabels { get; } = null;

    ConfigEntryBase ISettingEntry.Config => Primary;

    public KeybindEntry(
        string id,
        ConfigEntry<KeyCode> primary,
        ConfigEntry<KeyCode>? secondary,
        string label,
        SettingLocation location,
        Action? onPressed,
        Action? onReleased)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (primary is null)
            throw new ArgumentNullException(nameof(primary));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));

        Id = id;
        Primary = primary;
        Secondary = secondary;
        Label = label;
        Location = location;
        OnPressed = onPressed;
        OnReleased = onReleased;

        Primary.SettingChanged += (_, _) => ValueChanged?.Invoke();
        if (Secondary is not null)
            Secondary.SettingChanged += (_, _) => ValueChanged?.Invoke();
    }
}
