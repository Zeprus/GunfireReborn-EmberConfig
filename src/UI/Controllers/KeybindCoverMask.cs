namespace EmberConfig.UI;

using TMPro;
using UnityEngine;

/// <summary>
/// Manages the vanilla "press any key" cover mask used while rebinding a key.
/// </summary>
internal sealed class KeybindCoverMask
{
    private readonly KeybindButtonStyle style;

    private GameObject? mask;
    private TextMeshProUGUI? keyText;
    private TextMeshProUGUI? tipText;

    public KeybindCoverMask(KeybindButtonStyle style)
    {
        this.style = style;
    }

    /// <summary>
    /// Whether the mask is currently active and usable.
    /// </summary>
    public bool IsVisible => mask is not null && !mask.Equals(null) && mask.activeSelf;

    /// <summary>
    /// Shows the cover mask over the settings panel.
    /// </summary>
    /// <param name="panelRoot">The root of the vanilla settings panel.</param>
    /// <param name="label">The setting label to display in the prompt.</param>
    public void Show(Transform panelRoot, string label)
    {
        if (mask is null || mask.Equals(null))
        {
            mask = panelRoot.Find("bg_windows/setting_scroll/Viewport/covery_mask")?.gameObject;
            if (mask is null) return;

            keyText = mask.transform.Find("bg/key_txt")?.GetComponent<TextMeshProUGUI>();
            tipText = mask.transform.Find("bg/key_tip")?.GetComponent<TextMeshProUGUI>();
        }

        SetLogicEnabled(false);
        mask.SetActive(true);

        var highlightTag = $"<color=#{ColorUtility.ToHtmlStringRGB(style.NoneText.Color)}>";
        var endTag = "</color>";
        var escapeSprite = $"<sprite name=sck{(int)KeyCode.Escape}>";

        if (keyText is not null)
        {
            keyText.spriteAsset = style.SpriteAsset;
            TextAppearanceApplier.Apply(keyText, style.Text);
            keyText.text = $"Bind {highlightTag}{label}{endTag}\n<size=20>Press {highlightTag}[any key]{endTag} to bind or press {highlightTag}[{escapeSprite}]{endTag} to cancel";
        }

        if (tipText is not null)
        {
            tipText.spriteAsset = style.SpriteAsset;
            TextAppearanceApplier.Apply(tipText, style.Text);
            tipText.text = $"Press {highlightTag}[{escapeSprite}]{endTag} to unbind";
        }
    }

    /// <summary>
    /// Hides the cover mask and re-enables the vanilla keyboard swap logic.
    /// </summary>
    public void Hide()
    {
        if (mask is null || mask.Equals(null)) return;

        SetLogicEnabled(true);
        mask.SetActive(false);
    }

    private void SetLogicEnabled(bool enabled)
    {
        if (mask is null || mask.Equals(null)) return;

        var behaviours = mask.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (b is null || b.Equals(null)) continue;

            var typeName = b.GetIl2CppType().Name;
            if (typeName is "KeyboardSwapPanel_Logic")
                b.enabled = enabled;
        }
    }

}
