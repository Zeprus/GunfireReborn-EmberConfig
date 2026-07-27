namespace SettingsLib.Core;

using System;

/// <summary>
/// Internal representation of a registered keybind for the input dispatcher.
/// </summary>
public interface IKeybindEntry
{
    /// <summary>The display label for the keybind.</summary>
    string Label { get; }

    /// <summary>The integer value of the primary <see cref="UnityEngine.KeyCode"/>.</summary>
    int PrimaryKeyCodeValue { get; }

    /// <summary>The integer value of the secondary <see cref="UnityEngine.KeyCode"/>, if any.</summary>
    int? SecondaryKeyCodeValue { get; }

    /// <summary>Callback invoked when the keybind is pressed.</summary>
    Action? OnPressed { get; }

    /// <summary>Callback invoked when the keybind is released.</summary>
    Action? OnReleased { get; }
}
