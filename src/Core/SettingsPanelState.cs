namespace SettingsLib.Core;

using System;

/// <summary>
/// Shared runtime state for the settings panel. Patches and UI controllers read
/// this instead of reaching for the <see cref="SettingsMenuManager"/> singleton.
/// </summary>
public static class SettingsPanelState
{
    /// <summary>
    /// Whether any keybind row is currently capturing input.
    /// </summary>
    public static bool IsCapturing { get; set; }

    /// <summary>
    /// Whether the settings panel close/back actions should be blocked.
    /// Used while a keybind row is awaiting input.
    /// </summary>
    public static bool IsBlockingClose { get; set; }

    /// <summary>
    /// Raised when the native keyboard/keybind panel is refreshed.
    /// The argument is the native panel ID and is currently unused.
    /// </summary>
    public static event Action<int>? KeybindPanelRefreshed;

    /// <summary>
    /// Raises <see cref="KeybindPanelRefreshed"/>.
    /// </summary>
    /// <param name="id">The native panel ID.</param>
    public static void RefreshKeybindPanel(int id) => KeybindPanelRefreshed?.Invoke(id);
}
