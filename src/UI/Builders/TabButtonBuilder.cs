namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class TabButtonBuilder
{
    /// <summary>
    /// Builds a generic selectable tab button that is sized/positioned by a
    /// parent <see cref="HorizontalLayoutGroup"/>.
    /// </summary>
    /// <param name="name">GameObject name for the new tab.</param>
    /// <param name="label">Text shown on the tab.</param>
    /// <param name="tabStyle">Captured tab style.</param>
    /// <param name="parent">Transform to parent the new tab under.</param>
    /// <returns>The created <see cref="M1Toggle"/>.</returns>
    internal static M1Toggle Build(string name, string label, TabStyle tabStyle, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        RowElementBuilder.SetRect(rect, Vector2.zero, Vector2.zero, new Vector2(tabStyle.Width, tabStyle.Height), Vector2.zero);

        _ = go.AddComponent<CanvasRenderer>();

        // Invisible hit target for the whole tab. The M1Toggle will receive clicks here.
        var hitImage = go.AddComponent<Image>();
        hitImage.sprite = null;
        hitImage.color = Color.clear;
        hitImage.raycastTarget = true;

        // LayoutElement gives the parent HorizontalLayoutGroup a fixed size to work with.
        var layout = go.AddComponent<LayoutElement>();
        layout.minWidth = tabStyle.Width;
        layout.preferredWidth = tabStyle.Width;
        layout.minHeight = tabStyle.Height;
        layout.preferredHeight = tabStyle.Height;
        layout.flexibleWidth = 0f;
        layout.flexibleHeight = 0f;

        var toggle = go.AddComponent<M1Toggle>();
        toggle.targetGraphic = hitImage;
        toggle.graphic = null;
        toggle.ungraphic = null;
        toggle.transition = Selectable.Transition.None;
        toggle.interactable = true;
        toggle.m_Group = null;

        // Selected state background: the dark chevron sprite vanilla tabs show when active.
        var backgroundObj = RowElementBuilder.CreateObject("Background", go.transform);
        RowElementBuilder.SetRect(backgroundObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var checkmarkObj = RowElementBuilder.CreateObject("Checkmark", backgroundObj.transform);
        var checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
        if (tabStyle.SelectedBackgroundRect.HasValue)
            tabStyle.SelectedBackgroundRect.Value.Apply(checkmarkRect);
        else
            RowElementBuilder.SetRect(checkmarkObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var checkmarkImage = RowElementBuilder.AddImage(checkmarkObj, tabStyle.SelectedBackgroundSprite, Image.Type.Simple, Color.white);
        checkmarkImage.raycastTarget = false;

        // The tab label is always on top and changes color between unselected/selected states.
        var typeNameObj = RowElementBuilder.CreateObject("type_name", go.transform);
        RowElementBuilder.SetRect(typeNameObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var typeNameText = RowElementBuilder.AddText(typeNameObj, tabStyle.Unselected, label);
        typeNameText.raycastTarget = false;
        typeNameText.enableAutoSizing = true;
        typeNameText.overflowMode = TextOverflowModes.Ellipsis;
        typeNameText.fontSizeMin = 10f;
        typeNameText.fontSizeMax = tabStyle.Unselected.FontSize > 0f ? tabStyle.Unselected.FontSize : 30f;

        backgroundObj.SetActive(false);

        VanillaComponentApplier.ApplyToControl(go.transform, addDySelect: true, addAudio: true);

        toggle.SetIsOnWithoutNotify(false);

        go.SetActive(true);
        return toggle;
    }
}
