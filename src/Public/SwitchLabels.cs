namespace EmberConfig.Public;

/// <summary>
/// Allows a mod author to override the On/Off labels shown by a <see cref="SettingControlStyle.Switch"/> control.
/// </summary>
/// <param name="On">The label shown for the <c>true</c> option.</param>
/// <param name="Off">The label shown for the <c>false</c> option.</param>
public sealed record SwitchLabels(string On, string Off);
