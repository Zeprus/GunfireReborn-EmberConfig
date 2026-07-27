namespace SettingsLib.Public;

using System;
using BepInEx.Configuration;
using SettingsLib.Core;
using UnityEngine;

/// <summary>
/// Public entry point for registering settings rows that will be injected into
/// the native Gunfire Reborn settings panel.
/// </summary>
public static class SettingsMenu
{
    /// <summary>
    /// Registers a scalar setting and binds it to a new <see cref="ConfigEntry{T}"/>.
    /// </summary>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <param name="configFile">The config file to bind the entry to.</param>
    /// <param name="section">The config section.</param>
    /// <param name="key">The config key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="description">The setting description shown on hover.</param>
    /// <param name="label">The label shown in the settings row.</param>
    /// <param name="tab">The tab to place the setting under. Can be a native tab string or a custom tab name.</param>
    /// <param name="group">The group (usually the mod name) the setting belongs to.</param>
    /// <param name="subGroup">Optional sub-group within <paramref name="group"/>.</param>
    /// <param name="onValueChanged">Optional callback invoked when the value changes.</param>
    /// <param name="acceptableValues">Optional acceptable values (ranges, lists, etc.).</param>
    /// <returns>The bound <see cref="ConfigEntry{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configFile"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is <see cref="KeyCode"/>.</exception>
    public static ConfigEntry<T> Register<T>(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        string description,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null,
        AcceptableValueBase? acceptableValues = null)
    {
        if (configFile is null)
            throw new ArgumentNullException(nameof(configFile));
        if (typeof(T) == typeof(KeyCode))
            throw new ArgumentException("KeyCode settings must be registered with RegisterKeybind.", nameof(defaultValue));

        var configDescription = new ConfigDescription(description ?? string.Empty, acceptableValues);
        var config = configFile.Bind(section, key, defaultValue, configDescription);

        return Register(config, label, tab, group, subGroup, onValueChanged);
    }

    /// <summary>
    /// Registers a scalar setting using an existing <see cref="ConfigEntry{T}"/>.
    /// </summary>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <param name="config">The existing config entry.</param>
    /// <param name="label">The label shown in the settings row.</param>
    /// <param name="tab">The tab to place the setting under. Can be a native tab string or a custom tab name.</param>
    /// <param name="group">The group (usually the mod name) the setting belongs to.</param>
    /// <param name="subGroup">Optional sub-group within <paramref name="group"/>.</param>
    /// <param name="onValueChanged">Optional callback invoked when the value changes.</param>
    /// <returns>The <see cref="ConfigEntry{T}"/> that was passed in.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is empty or <typeparamref name="T"/> is <see cref="KeyCode"/>.</exception>
    public static ConfigEntry<T> Register<T>(
        ConfigEntry<T> config,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));
        if (typeof(T) == typeof(KeyCode))
            throw new ArgumentException("KeyCode settings must be registered with RegisterKeybind.", nameof(config));

        var location = CreateLocation(tab, group, subGroup);
        var id = Guid.NewGuid().ToString("N");
        var entry = new SettingEntry<T>(id, config, label, location, onValueChanged);

        SettingsRegistry.Current.Register(entry);
        return config;
    }

    /// <summary>
    /// Registers a keybind and binds it to new <see cref="ConfigEntry{KeyCode}"/> entries.
    /// </summary>
    /// <param name="configFile">The config file to bind the entries to.</param>
    /// <param name="section">The config section.</param>
    /// <param name="key">The config key for the primary binding.</param>
    /// <param name="defaultPrimary">The default primary key.</param>
    /// <param name="description">The keybind description shown on hover.</param>
    /// <param name="label">The label shown in the settings row.</param>
    /// <param name="tab">The tab to place the keybind under. Can be a native tab string or a custom tab name.</param>
    /// <param name="group">The group (usually the mod name) the keybind belongs to.</param>
    /// <param name="subGroup">Optional sub-group within <paramref name="group"/>.</param>
    /// <param name="defaultSecondary">Optional default secondary key.</param>
    /// <param name="onPressed">Optional callback invoked when the keybind is pressed.</param>
    /// <param name="onReleased">Optional callback invoked when the keybind is released.</param>
    /// <returns>A <see cref="KeybindRegistration"/> containing the primary and optional secondary entries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configFile"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
    public static KeybindRegistration RegisterKeybind(
        ConfigFile configFile,
        string section,
        string key,
        KeyCode defaultPrimary,
        string description,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        KeyCode? defaultSecondary = null,
        Action? onPressed = null,
        Action? onReleased = null)
    {
        if (configFile is null)
            throw new ArgumentNullException(nameof(configFile));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty.", nameof(key));

        var configDescription = new ConfigDescription(description ?? string.Empty, null);
        var primary = configFile.Bind(section, key, defaultPrimary, configDescription);

        ConfigEntry<KeyCode>? secondary = null;
        if (defaultSecondary.HasValue)
            secondary = configFile.Bind(section, $"{key}Secondary", defaultSecondary.Value, configDescription);

        return RegisterKeybind(primary, secondary, label, tab, group, subGroup, onPressed, onReleased);
    }

    /// <summary>
    /// Registers a keybind using existing <see cref="ConfigEntry{KeyCode}"/> entries.
    /// </summary>
    /// <param name="primary">The primary key binding.</param>
    /// <param name="secondary">Optional secondary key binding.</param>
    /// <param name="label">The label shown in the settings row.</param>
    /// <param name="tab">The tab to place the keybind under. Can be a native tab string or a custom tab name.</param>
    /// <param name="group">The group (usually the mod name) the keybind belongs to.</param>
    /// <param name="subGroup">Optional sub-group within <paramref name="group"/>.</param>
    /// <param name="onPressed">Optional callback invoked when the keybind is pressed.</param>
    /// <param name="onReleased">Optional callback invoked when the keybind is released.</param>
    /// <returns>A <see cref="KeybindRegistration"/> containing the primary and optional secondary entries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="primary"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is empty.</exception>
    public static KeybindRegistration RegisterKeybind(
        ConfigEntry<KeyCode> primary,
        ConfigEntry<KeyCode>? secondary,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action? onPressed = null,
        Action? onReleased = null)
    {
        if (primary is null)
            throw new ArgumentNullException(nameof(primary));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));

        var location = CreateLocation(tab, group, subGroup);
        var id = Guid.NewGuid().ToString("N");
        var entry = new KeybindEntry(id, primary, secondary, label, location, onPressed, onReleased);

        SettingsRegistry.Current.Register(entry);
        return new KeybindRegistration(primary, secondary);
    }

    /// <inheritdoc cref="Register{T}(ConfigFile, string, string, T, string, string, string, string, string?, Action{T}?, AcceptableValueBase?)" />
    /// <param name="tab">The native settings tab to place the setting under.</param>
    public static ConfigEntry<T> Register<T>(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        string description,
        string label,
        SettingsTab tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null,
        AcceptableValueBase? acceptableValues = null)
    {
        return Register(configFile, section, key, defaultValue, description, label, tab.ToNativeName(), group, subGroup, onValueChanged, acceptableValues);
    }

    /// <inheritdoc cref="Register{T}(ConfigEntry{T}, string, string, string, string?, Action{T}?)" />
    /// <param name="tab">The native settings tab to place the setting under.</param>
    public static ConfigEntry<T> Register<T>(
        ConfigEntry<T> config,
        string label,
        SettingsTab tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null)
    {
        return Register(config, label, tab.ToNativeName(), group, subGroup, onValueChanged);
    }

    /// <inheritdoc cref="RegisterKeybind(ConfigFile, string, string, KeyCode, string, string, string, string, string?, KeyCode?, Action?, Action?)" />
    /// <param name="tab">The native settings tab to place the keybind under.</param>
    public static KeybindRegistration RegisterKeybind(
        ConfigFile configFile,
        string section,
        string key,
        KeyCode defaultPrimary,
        string description,
        string label,
        SettingsTab tab,
        string group,
        string? subGroup = null,
        KeyCode? defaultSecondary = null,
        Action? onPressed = null,
        Action? onReleased = null)
    {
        return RegisterKeybind(configFile, section, key, defaultPrimary, description, label, tab.ToNativeName(), group, subGroup, defaultSecondary, onPressed, onReleased);
    }

    /// <inheritdoc cref="RegisterKeybind(ConfigEntry{KeyCode}, ConfigEntry{KeyCode}?, string, string, string, string?, Action?, Action?)" />
    /// <param name="tab">The native settings tab to place the keybind under.</param>
    public static KeybindRegistration RegisterKeybind(
        ConfigEntry<KeyCode> primary,
        ConfigEntry<KeyCode>? secondary,
        string label,
        SettingsTab tab,
        string group,
        string? subGroup = null,
        Action? onPressed = null,
        Action? onReleased = null)
    {
        return RegisterKeybind(primary, secondary, label, tab.ToNativeName(), group, subGroup, onPressed, onReleased);
    }

    private static SettingLocation CreateLocation(string tab, string group, string? subGroup)
    {
        if (string.IsNullOrWhiteSpace(tab))
            throw new ArgumentException("Tab cannot be empty.", nameof(tab));
        if (string.IsNullOrWhiteSpace(group))
            throw new ArgumentException("Group cannot be empty.", nameof(group));

        return new SettingLocation(tab.Trim(), group.Trim(), string.IsNullOrWhiteSpace(subGroup) ? null : subGroup.Trim());
    }
}
