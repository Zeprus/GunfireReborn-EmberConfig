namespace EmberConfig.Public;

/// <summary>
/// Native Gunfire Reborn settings tabs. These are the tabs that are guaranteed
/// to exist in the vanilla settings panel. Custom tab names can still be passed
/// as strings to the <see cref="SettingsMenu"/> registration methods.
/// </summary>
public enum SettingsTab
{
    /// <summary>The Game Settings tab.</summary>
    GameSettings,

    /// <summary>The Mouse/Keyboard settings tab.</summary>
    MouseKeyboard,

    /// <summary>The Video settings tab.</summary>
    Video,

    /// <summary>The Audio settings tab.</summary>
    Audio,

    /// <summary>The Controller settings tab.</summary>
    Controller
}

/// <summary>
/// Extension methods for <see cref="SettingsTab"/>.
/// </summary>
public static class SettingsTabExtensions
{
    /// <summary>
    /// Returns the exact native tab name used by the Gunfire Reborn UI.
    /// </summary>
    /// <param name="tab">The tab to resolve.</param>
    /// <returns>The native tab name string.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tab"/> is not a defined value.
    /// </exception>
    public static string ToNativeName(this SettingsTab tab) => tab switch
    {
        SettingsTab.GameSettings => "Game Settings",
        SettingsTab.MouseKeyboard => "Mouse/Keyboard",
        SettingsTab.Video => "Video",
        SettingsTab.Audio => "Audio",
        SettingsTab.Controller => "Controller",
        _ => throw new System.ArgumentOutOfRangeException(nameof(tab))
    };
}
