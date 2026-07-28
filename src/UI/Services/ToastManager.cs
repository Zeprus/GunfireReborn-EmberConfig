namespace SettingsLib.UI;

using TMPro;
using UnityEngine;

/// <summary>
/// Displays and hides the vanilla "change button" toast when a keybind is captured.
/// </summary>
internal sealed class ToastManager
{
    private GameObject? activeToast;
    private float toastTimer;

    /// <summary>
    /// Shows the keybind toast for the given key.
    /// </summary>
    /// <param name="rowTransform">The row that triggered the toast.</param>
    /// <param name="label">The setting label to display.</param>
    /// <param name="key">The bound key.</param>
    public void Show(Transform rowTransform, string label, KeyCode key)
    {
        var panel = PanelLocator.FindPanelRoot(rowTransform);
        if (panel is null) return;

        var toast = panel.Find("bg_windows/changebutton_tips/1")?.gameObject;
        if (toast is null) return;

        var keyText = toast.transform.Find("SCK_tips_desc")?.GetComponent<TextMeshProUGUI>();
        var nameText = toast.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();

        if (keyText is not null)
            keyText.text = key == KeyCode.None ? "None" : $"<sprite name=sck{(int)key}>";

        if (nameText is not null)
            nameText.text = label;

        toast.SetActive(true);
        activeToast = toast;
        toastTimer = 2f;
    }

    /// <summary>
    /// Should be called each frame to hide the toast after a short delay.
    /// </summary>
    public void Update()
    {
        if (activeToast is null || activeToast.Equals(null))
            return;

        toastTimer -= Time.deltaTime;
        if (toastTimer <= 0f)
        {
            activeToast.SetActive(false);
            activeToast = null;
        }
    }
}
