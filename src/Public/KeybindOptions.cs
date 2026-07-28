namespace EmberConfig.Public;

using System;
using UnityEngine;

/// <summary>
/// Describes how a keybind should be registered and displayed in the
/// native Gunfire Reborn settings panel.
/// </summary>
public sealed record KeybindOptions(
    string Section,
    string Key,
    KeyCode DefaultPrimary,
    string Description,
    string Label,
    string Tab,
    string Group,
    string? SubGroup = null,
    KeyCode? DefaultSecondary = null,
    Action? OnPressed = null,
    Action? OnReleased = null)
{
    /// <summary>
    /// Returns a copy of these options with <see cref="Tab"/> set to the
    /// native tab name for <paramref name="tab"/>.
    /// </summary>
    public KeybindOptions WithTab(SettingsTab tab) =>
        this with { Tab = tab.ToNativeName() };
}
