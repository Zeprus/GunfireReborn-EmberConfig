namespace SettingsLib.Public;

using BepInEx.Configuration;
using UnityEngine;

/// <summary>
/// The result of registering a keybind. Contains the primary and optional
/// secondary <see cref="ConfigEntry{KeyCode}"/> instances.
/// </summary>
/// <param name="Primary">The primary key binding config entry.</param>
/// <param name="Secondary">The optional secondary key binding config entry, or <c>null</c> if none was registered.</param>
public sealed record KeybindRegistration(ConfigEntry<KeyCode> Primary, ConfigEntry<KeyCode>? Secondary);
