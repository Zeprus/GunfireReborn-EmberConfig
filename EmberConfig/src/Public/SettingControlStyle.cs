namespace EmberConfig.Public;

/// <summary>
/// Allows a mod author to choose the visual control style for a setting row.
/// <see cref="Auto"/> lets EmberConfig pick the default style based on the value type.
/// </summary>
public enum SettingControlStyle
{
    Auto,
    Switch,
    Dropdown,
    Carousel,
}
