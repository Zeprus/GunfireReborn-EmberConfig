namespace EmberConfig.UI;

using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Guard component attached to modded dropdowns. It refreshes the caption text
/// whenever the dropdown is enabled/started so the selected value is visible
/// even when other vanilla components clear the label during initialization.
/// </summary>
public class DropdownCaptionGuard : MonoBehaviour
{
    public DropdownCaptionGuard(IntPtr ptr) : base(ptr) { }

    private TMP_Dropdown? dropdown;

    public void SetDropdown(TMP_Dropdown? target)
    {
        dropdown = target;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        try
        {
            dropdown?.RefreshShownValue();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"EmberConfig: DropdownCaptionGuard.Refresh failed: {ex.Message}");
        }
    }
}
