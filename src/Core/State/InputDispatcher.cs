namespace EmberConfig.Core;

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InputDispatcher
{
    private readonly Func<IEnumerable<IKeybindEntry>> getKeybinds;
    private readonly Func<int, bool> getKeyDown;
    private readonly Func<int, bool> getKeyUp;

    public InputDispatcher(
        Func<IEnumerable<IKeybindEntry>> getKeybinds,
        Func<int, bool>? getKeyDown = null,
        Func<int, bool>? getKeyUp = null)
    {
        this.getKeybinds = getKeybinds ?? throw new ArgumentNullException(nameof(getKeybinds));
        this.getKeyDown = getKeyDown ?? (k => k != 0 && Input.GetKeyDown((KeyCode)k));
        this.getKeyUp = getKeyUp ?? (k => k != 0 && Input.GetKeyUp((KeyCode)k));
    }

    public void Poll(bool canDispatch)
    {
        if (!canDispatch || SettingsPanelState.IsCapturing)
            return;

        foreach (var entry in getKeybinds())
        {
            bool pressed = IsPressed(entry);
            bool released = IsReleased(entry);

            if (entry.OnPressed is not null && pressed)
            {
                try { entry.OnPressed(); }
                catch (Exception ex) { Plugin.Logger?.LogError($"Keybind press for '{entry.Label}' failed: {ex}"); }
            }

            if (entry.OnReleased is not null && released)
            {
                try { entry.OnReleased(); }
                catch (Exception ex) { Plugin.Logger?.LogError($"Keybind release for '{entry.Label}' failed: {ex}"); }
            }
        }
    }

    private bool IsPressed(IKeybindEntry entry)
    {
        if (entry.PrimaryKeyCodeValue != 0 && getKeyDown(entry.PrimaryKeyCodeValue))
            return true;

        return entry.SecondaryKeyCodeValue is { } secondary
            && secondary != 0
            && getKeyDown(secondary);
    }

    private bool IsReleased(IKeybindEntry entry)
    {
        if (entry.PrimaryKeyCodeValue != 0 && getKeyUp(entry.PrimaryKeyCodeValue))
            return true;

        return entry.SecondaryKeyCodeValue is { } secondary
            && secondary != 0
            && getKeyUp(secondary);
    }
}
