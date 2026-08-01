namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

internal static class KeybindElementBuilder
{
    internal static Transform Build(string name, RowStyle rowStyle, KeybindButtonStyle keybindStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, rowStyle, parent);
        RowElementBuilder.CreateTitle(rowStyle, root);

        var item = RowElementBuilder.CreateItem(rowStyle, root);
        keybindStyle.ItemRect.Apply(item.GetComponent<RectTransform>());

        var hlg = item.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = keybindStyle.ItemLayout.Spacing;
        hlg.childAlignment = keybindStyle.ItemLayout.ChildAlignment;
        hlg.childForceExpandWidth = keybindStyle.ItemLayout.ChildForceExpandWidth;
        hlg.childForceExpandHeight = keybindStyle.ItemLayout.ChildForceExpandHeight;
        hlg.childControlWidth = keybindStyle.ItemLayout.ChildControlWidth;
        hlg.childControlHeight = keybindStyle.ItemLayout.ChildControlHeight;
        hlg.padding = new RectOffset(
            keybindStyle.ItemLayout.PaddingLeft,
            keybindStyle.ItemLayout.PaddingRight,
            keybindStyle.ItemLayout.PaddingTop,
            keybindStyle.ItemLayout.PaddingBottom);

        var btn1 = RowElementBuilder.CreateKeybindButton("change_button_1", keybindStyle, item.transform, isNone: false);
        keybindStyle.PrimaryRect.Apply(btn1.GetComponent<RectTransform>());

        var btn2 = RowElementBuilder.CreateKeybindButton("change_button_2", keybindStyle, item.transform, isNone: true);
        keybindStyle.SecondaryRect.Apply(btn2.GetComponent<RectTransform>());

        return root.transform;
    }
}
