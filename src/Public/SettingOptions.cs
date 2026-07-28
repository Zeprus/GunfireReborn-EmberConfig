namespace EmberConfig.Public;

using System;
using BepInEx.Configuration;

/// <summary>
/// Describes how a scalar setting should be registered and displayed in the
/// native Gunfire Reborn settings panel.
/// </summary>
/// <typeparam name="T">The setting value type.</typeparam>
public sealed record SettingOptions<T>(
    string Section,
    string Key,
    T DefaultValue,
    string Description,
    string Label,
    string Tab,
    string Group,
    string? SubGroup = null,
    Action<T>? OnValueChanged = null,
    AcceptableValueBase? AcceptableValues = null)
{
    /// <summary>
    /// Returns a copy of these options with <see cref="Tab"/> set to the
    /// native tab name for <paramref name="tab"/>.
    /// </summary>
    public SettingOptions<T> WithTab(SettingsTab tab) =>
        this with { Tab = tab.ToNativeName() };
}
