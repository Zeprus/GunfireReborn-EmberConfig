namespace EmberConfig.UI;

using UnityEngine;

/// <summary>
/// Services required by <see cref="KeybindRow"/> that cannot be handled by the
/// row itself (e.g. showing the native "change button" toast).
/// </summary>
internal interface IKeybindRowServices
{
    /// <summary>
    /// Displays the vanilla keybind toast for the given key.
    /// </summary>
    /// <param name="rowTransform">The row that triggered the toast.</param>
    /// <param name="label">The setting label to display.</param>
    /// <param name="key">The bound key.</param>
    void ShowKeybindToast(Transform rowTransform, string label, KeyCode key);
}
