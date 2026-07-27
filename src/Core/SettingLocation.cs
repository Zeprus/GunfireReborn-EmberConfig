namespace SettingsLib.Core;

/// <summary>
/// Identifies where a setting row should appear in the settings panel.
/// </summary>
/// <param name="Tab">The tab name.</param>
/// <param name="Group">The group name, typically the mod name.</param>
/// <param name="SubGroup">Optional sub-group used for additional headers.</param>
public readonly record struct SettingLocation(string Tab, string Group, string? SubGroup = null);
