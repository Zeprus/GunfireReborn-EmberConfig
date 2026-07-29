namespace EmberConfig.ExampleMod;

using System;
using UnityEngine;

/// <summary>
/// Runtime helper for the example mod. It owns the marker and overlay objects
/// that some example settings toggle, so those settings can do something
/// visible without patching the game itself.
/// </summary>
public class ExampleMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// A plain GameObject that example toggles can show or hide.
    /// </summary>
    public GameObject? Marker { get; private set; }

    /// <summary>
    /// A plain GameObject that example keybinds/toggles can show, hide, or toggle.
    /// </summary>
    public GameObject? Overlay { get; private set; }

    public ExampleMonoBehaviour(IntPtr pointer) : base(pointer)
    {
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Marker = new GameObject("ExampleMod_Marker");
        DontDestroyOnLoad(Marker);
        Marker.SetActive(false);

        Overlay = new GameObject("ExampleMod_Overlay");
        DontDestroyOnLoad(Overlay);
        Overlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Marker != null)
            UnityEngine.Object.Destroy(Marker);

        if (Overlay != null)
            UnityEngine.Object.Destroy(Overlay);
    }

    /// <summary>
    /// Shows or hides the marker object.
    /// </summary>
    public void SetMarkerVisible(bool visible)
    {
        if (Marker != null)
            Marker.SetActive(visible);
    }

    /// <summary>
    /// Shows or hides the overlay object.
    /// </summary>
    public void SetOverlayVisible(bool visible)
    {
        if (Overlay != null)
            Overlay.SetActive(visible);
    }

    /// <summary>
    /// Toggles the overlay object's active state.
    /// </summary>
    public void ToggleOverlay()
    {
        if (Overlay != null)
            Overlay.SetActive(!Overlay.activeSelf);
    }
}
