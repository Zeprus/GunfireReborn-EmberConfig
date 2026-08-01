namespace EmberConfig.Public;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
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
    /// <param name="options">The options describing the setting.</param>
    /// <returns>The bound <see cref="ConfigEntry{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configFile"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is <see cref="KeyCode"/>.</exception>
    public static ConfigEntry<T> Register<T>(ConfigFile configFile, SettingOptions<T> options)
    {
        configFile = RequireNonNull(configFile, nameof(configFile));
        options = RequireNonNull(options, nameof(options));
        ValidateModName(options.ModName, nameof(options));
        if (typeof(T) == typeof(KeyCode))
            throw new ArgumentException("KeyCode settings must be registered with RegisterKeybind.", nameof(options));

        var configDescription = new ConfigDescription(options.Description, options.AcceptableValues);
        var config = configFile.Bind(options.Section, options.Key, options.DefaultValue, configDescription);

        return Register(config, options);
    }

    /// <summary>
    /// Registers a scalar setting using an existing <see cref="ConfigEntry{T}"/>.
    /// </summary>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <param name="config">The existing config entry.</param>
    /// <param name="options">The options describing the setting.</param>
    /// <returns>The <see cref="ConfigEntry{T}"/> that was passed in.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options.Label"/> is empty or <typeparamref name="T"/> is <see cref="KeyCode"/>.</exception>
    public static ConfigEntry<T> Register<T>(ConfigEntry<T> config, SettingOptions<T> options)
    {
        config = RequireNonNull(config, nameof(config));
        options = RequireNonNull(options, nameof(options));
        ValidateModName(options.ModName, nameof(options));
        ValidateNotNullOrWhiteSpace(options.Label, nameof(options.Label));
        if (typeof(T) == typeof(KeyCode))
            throw new ArgumentException("KeyCode settings must be registered with RegisterKeybind.", nameof(config));

        var location = CreateLocation(options.Tab, options.Group, options.SubGroup);
        var id = Guid.NewGuid().ToString("N");
        var entry = new SettingEntry<T>(id, config, options.Label, options.ModName, location, options.OnValueChanged, options.ControlStyle, options.SwitchLabels);

        SettingsRegistry.Current.Register(entry);
        if (VisibilityStore.IsInitialized)
            VisibilityStore.Current.EnsureVisibilitySwitch(entry);
        return config;
    }

    /// <summary>
    /// Registers a keybind and binds it to new <see cref="ConfigEntry{KeyCode}"/> entries.
    /// </summary>
    /// <param name="configFile">The config file to bind the entries to.</param>
    /// <param name="options">The options describing the keybind.</param>
    /// <returns>A <see cref="KeybindRegistration"/> containing the primary and optional secondary entries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configFile"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options.Key"/> is empty.</exception>
    public static KeybindRegistration RegisterKeybind(ConfigFile configFile, KeybindOptions options)
    {
        configFile = RequireNonNull(configFile, nameof(configFile));
        options = RequireNonNull(options, nameof(options));
        ValidateModName(options.ModName, nameof(options));
        ValidateNotNullOrWhiteSpace(options.Key, nameof(options.Key));

        var configDescription = new ConfigDescription(options.Description ?? string.Empty, null);
        var primary = configFile.Bind(options.Section, options.Key, options.DefaultPrimary, configDescription);

        ConfigEntry<KeyCode>? secondary = null;
        if (options.DefaultSecondary.HasValue)
            secondary = configFile.Bind(options.Section, $"{options.Key}Secondary", options.DefaultSecondary.Value, configDescription);

        return RegisterKeybind(primary, secondary, options);
    }

    /// <summary>
    /// Registers a keybind using existing <see cref="ConfigEntry{KeyCode}"/> entries.
    /// </summary>
    /// <param name="primary">The primary key binding.</param>
    /// <param name="secondary">Optional secondary key binding.</param>
    /// <param name="options">The options describing the keybind.</param>
    /// <returns>A <see cref="KeybindRegistration"/> containing the primary and optional secondary entries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="primary"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options.Label"/> is empty.</exception>
    public static KeybindRegistration RegisterKeybind(ConfigEntry<KeyCode> primary, ConfigEntry<KeyCode>? secondary, KeybindOptions options)
    {
        primary = RequireNonNull(primary, nameof(primary));
        options = RequireNonNull(options, nameof(options));
        ValidateModName(options.ModName, nameof(options));
        ValidateNotNullOrWhiteSpace(options.Label, nameof(options.Label));

        var location = CreateLocation(options.Tab, options.Group, options.SubGroup);
        var id = Guid.NewGuid().ToString("N");
        var entry = new KeybindEntry(id, primary, secondary, options.Label, options.ModName, location, options.OnPressed, options.OnReleased);

        SettingsRegistry.Current.Register(entry);
        if (VisibilityStore.IsInitialized)
            VisibilityStore.Current.EnsureVisibilitySwitch(entry);
        return new KeybindRegistration(primary, secondary);
    }

    private static T RequireNonNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    private static void ValidateModName(string? modName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(modName))
            throw new ArgumentException("ModName cannot be empty.", paramName);
    }

    private static void ValidateNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
    }

    private static SettingLocation CreateLocation(string tab, string? group, string? subGroup)
    {
        if (string.IsNullOrWhiteSpace(tab))
            throw new ArgumentException("Tab cannot be empty.", nameof(tab));

        return new SettingLocation(tab.Trim(), string.IsNullOrWhiteSpace(group) ? null : group.Trim(), string.IsNullOrWhiteSpace(subGroup) ? null : subGroup.Trim());
    }
}
