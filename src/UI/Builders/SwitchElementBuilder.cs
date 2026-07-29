namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

internal static class SwitchElementBuilder
{
    internal static Transform Build(string name, RowStyle style, SwitchStyle switchStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var item = RowElementBuilder.CreateItem(style, root);
        var clickGroupObj = RowElementBuilder.CreateObject("ClickGroup", item.transform);
        switchStyle.ClickGroupRect.Apply(clickGroupObj.GetComponent<RectTransform>());

        var hlg = clickGroupObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = switchStyle.Spacing;
        hlg.childAlignment = switchStyle.ChildAlignment;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        var toggleGroup = clickGroupObj.AddComponent<M1ToggleGroup>();
        toggleGroup.allowSwitchOff = false;

        var first = BuildOption(switchStyle, clickGroupObj.transform, toggleGroup, switchStyle.Option1Label, 1);
        var second = BuildOption(switchStyle, clickGroupObj.transform, toggleGroup, switchStyle.Option2Label, 2);

        // Use the first option as the row's selectable for controller navigation.
        VanillaComponentApplier.ApplyToRow(root.transform, first);
        VanillaComponentApplier.AttachAudio(first.transform);
        VanillaComponentApplier.AttachAudio(second.transform, false);

        return root.transform;
    }

    private static M1Toggle BuildOption(SwitchStyle style, Transform clickGroup, M1ToggleGroup group, string label, int index)
    {
        var optionObj = RowElementBuilder.CreateObject(index.ToString(), clickGroup);
        var optionRect = optionObj.GetComponent<RectTransform>();
        optionRect.sizeDelta = style.OptionSize;
        optionRect.pivot = new Vector2(0.5f, 0.5f);
        optionRect.anchorMin = new Vector2(0.5f, 0.5f);
        optionRect.anchorMax = new Vector2(0.5f, 0.5f);
        optionRect.anchoredPosition = Vector2.zero;

        var layoutEl = optionObj.AddComponent<LayoutElement>();
        layoutEl.minWidth = style.OptionSize.x;
        layoutEl.minHeight = style.OptionSize.y;
        layoutEl.preferredWidth = style.OptionSize.x;
        layoutEl.preferredHeight = style.OptionSize.y;

        var toggle = optionObj.AddComponent<M1Toggle>();
        toggle.onValueChanged ??= new M1Toggle.ToggleEvent();
        toggle.group = group;
        toggle.interactable = true;

        var bgObj = RowElementBuilder.CreateObject("Background", optionObj.transform);
        RowElementBuilder.SetRect(bgObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var bgImage = RowElementBuilder.AddImage(bgObj, style.OptionBackgroundSprite, style.OptionBackgroundType, style.OptionBackgroundColor);
        toggle.targetGraphic = bgImage;

        var checkObj = RowElementBuilder.CreateObject("Checkmark", bgObj.transform);
        RowElementBuilder.SetRect(checkObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var checkImage = RowElementBuilder.AddImage(checkObj, style.OptionCheckmarkSprite, style.OptionCheckmarkType, style.OptionCheckmarkColor);
        toggle.graphic = checkImage;
        toggle.ungraphic = null;

        var labelObj = RowElementBuilder.CreateObject("Label", optionObj.transform);
        RowElementBuilder.SetRect(labelObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var labelText = RowElementBuilder.AddText(labelObj, style.LabelTextAppearance, label);
        labelText.raycastTarget = false;

        return toggle;
    }
}
