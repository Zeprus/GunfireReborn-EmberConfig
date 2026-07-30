namespace EmberConfig.UI;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal static class RowElementBuilder
{
    internal static class Metrics
    {
        internal const float ControlHeight = 40f;
        internal const float DropdownItemHeight = 28f;
    }

    internal static GameObject CreateRowRoot(string name, RowStyle style, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        style.RowRect.Apply(rect);

        go.AddComponent<CanvasRenderer>();
        var image = go.AddComponent<Image>();
        image.sprite = style.BackgroundSprite;
        image.type = style.BackgroundType;
        image.color = style.BackgroundColor;

        var hover = go.AddComponent<RowHoverHandler>();
        hover.Background = image;
        hover.NormalColor = style.BackgroundColor;
        hover.HighlightColor = style.HighlightColor;

        var layoutEl = go.AddComponent<LayoutElement>();
        layoutEl.preferredHeight = style.Height;

        go.SetActive(true);
        return go;
    }

    internal static GameObject CreateTitle(RowStyle style, GameObject parent)
    {
        var go = CreateObject("Title", parent.transform);
        style.TitleRect.Apply(go.GetComponent<RectTransform>());

        var text = AddText(go, style.Title, string.Empty);
        text.raycastTarget = false;

        return go;
    }

    internal static GameObject CreateItem(RowStyle style, GameObject parent)
    {
        var go = CreateObject("Item", parent.transform);
        style.ItemRect.Apply(go.GetComponent<RectTransform>());
        return go;
    }

    internal static GameObject CreateKeybindButton(string name, KeybindButtonStyle style, Transform parent, bool isNone = false)
    {
        var go = CreateObject(name, parent);
        go.AddComponent<CanvasRenderer>();
        var image = go.AddComponent<Image>();
        image.sprite = style.BackgroundSprite;
        image.type = style.BackgroundType;
        image.color = style.BackgroundColor;

        var textObj = CreateObject("Text_1", go.transform);
        SetRect(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var appearance = isNone ? style.NoneText : style.Text;
        var tmp = AddText(textObj, appearance, "None");
        tmp.raycastTarget = false;

        var button = go.AddComponent<M1Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = style.ButtonColors;
        button.interactable = true;

        VanillaComponentApplier.ApplyToControl(go.transform, addDySelect: true, addAudio: true);

        return go;
    }

    internal static GameObject CreateObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        return go;
    }

    internal static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax, sizeDelta, anchoredPosition);
    }

    internal static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
    }

    internal static TextMeshProUGUI AddText(GameObject go, TextAppearance appearance, string text, TextAlignmentOptions? alignment = null)
    {
        go.AddComponent<CanvasRenderer>();
        var tmp = go.AddComponent<TextMeshProUGUI>();

        tmp.font = appearance.Font;
        if (appearance.Font is not null && appearance.FontMaterial == appearance.Font.material)
            tmp.fontSharedMaterial = appearance.FontMaterial;
        tmp.text = text;
        tmp.alignment = alignment ?? appearance.Alignment;
        tmp.fontStyle = appearance.FontStyle;
        tmp.fontSize = appearance.FontSize;
        tmp.fontSizeMin = appearance.FontSizeMin;
        tmp.fontSizeMax = appearance.FontSizeMax;
        tmp.color = appearance.Color;
        tmp.outlineWidth = appearance.OutlineWidth;
        tmp.enableWordWrapping = appearance.EnableWordWrapping;
        tmp.enableAutoSizing = appearance.EnableAutoSizing;
        tmp.overflowMode = appearance.OverflowMode;
        tmp.raycastTarget = false;
        return tmp;
    }

    internal static Image AddImage(GameObject go, Sprite? sprite, Image.Type type, Color color)
    {
        go.AddComponent<CanvasRenderer>();
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = color;
        return image;
    }
}
