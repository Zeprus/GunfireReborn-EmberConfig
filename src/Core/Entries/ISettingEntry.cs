namespace EmberConfig.Core;

using System;
using BepInEx.Configuration;

/// <summary>
/// Internal representation of a registered setting row.
/// </summary>
public interface ISettingEntry
{
    /// <summary>Unique identifier for this entry.</summary>
    string Id { get; }

    /// <summary>The underlying BepInEx config entry.</summary>
    ConfigEntryBase Config { get; }

    /// <summary>The display label for the row.</summary>
    string Label { get; }

    /// <summary>The description shown on hover.</summary>
    string Description => Config.Description?.Description ?? string.Empty;

    /// <summary>The tab/group/sub-group location of the row.</summary>
    SettingLocation Location { get; }

    /// <summary>Raised when the config value changes.</summary>
    event Action? ValueChanged;
}
