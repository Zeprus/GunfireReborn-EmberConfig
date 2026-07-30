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
        hlg.spacing = switchStyle.ClickGroupLayout.Spacing;
        hlg.childAlignment = switchStyle.ClickGroupLayout.ChildAlignment;
        hlg.childForceExpandWidth = switchStyle.ClickGroupLayout.ChildForceExpandWidth;
        hlg.childForceExpandHeight = switchStyle.ClickGroupLayout.ChildForceExpandHeight;
        hlg.childControlWidth = switchStyle.ClickGroupLayout.ChildControlWidth;
        hlg.childControlHeight = switchStyle.ClickGroupLayout.ChildControlHeight;
        hlg.padding = new RectOffset(
            switchStyle.ClickGroupLayout.PaddingLeft,
            switchStyle.ClickGroupLayout.PaddingRight,
            switchStyle.ClickGroupLayout.PaddingTop,
            switchStyle.ClickGroupLayout.PaddingBottom);

        var toggleGroup = clickGroupObj.AddComponent<M1ToggleGroup>();
        toggleGroup.allowSwitchOff = switchStyle.AllowSwitchOff;

        var first = BuildOption(switchStyle, clickGroupObj.transform, toggleGroup, switchStyle.Option1Label, 1);
        var second = BuildOption(switchStyle, clickGroupObj.transform, toggleGroup, switchStyle.Option2Label, 2);

        VanillaComponentApplier.ApplyToRow(root.transform, first);
        VanillaComponentApplier.ApplyToControl(first.transform, addDySelect: true, addAudio: true);
        VanillaComponentApplier.ApplyToControl(second.transform, addDySelect: true, addAudio: false);

        return root.transform;
    }

    private static M1Toggle BuildOption(SwitchStyle style, Transform clickGroup, M1ToggleGroup group, string label, int index)
    {
        var optionObj = RowElementBuilder.CreateObject(index.ToString(), clickGroup);
        style.OptionRect.Apply(optionObj.GetComponent<RectTransform>());

        var layoutEl = optionObj.AddComponent<LayoutElement>();
        layoutEl.minWidth = style.OptionRect.SizeDelta.x;
        layoutEl.minHeight = style.OptionRect.SizeDelta.y;
        layoutEl.preferredWidth = style.OptionRect.SizeDelta.x;
        layoutEl.preferredHeight = style.OptionRect.SizeDelta.y;

        var toggle = optionObj.AddComponent<M1Toggle>();
        toggle.onValueChanged ??= new M1Toggle.ToggleEvent();
        toggle.group = group;
        toggle.interactable = true;
        toggle.transition = style.OptionTransition;
        toggle.colors = style.OptionColorBlock;

        var bgObj = RowElementBuilder.CreateObject("Background", optionObj.transform);
        style.BackgroundRect.Apply(bgObj.GetComponent<RectTransform>());
        var bgImage = RowElementBuilder.AddImage(bgObj, style.OptionBackgroundSprite, style.OptionBackgroundType, style.OptionBackgroundColor);
        toggle.targetGraphic = bgImage;

        var checkObj = RowElementBuilder.CreateObject("Checkmark", bgObj.transform);
        style.CheckmarkRect.Apply(checkObj.GetComponent<RectTransform>());
        var checkImage = RowElementBuilder.AddImage(checkObj, style.OptionCheckmarkSprite, style.OptionCheckmarkType, style.OptionCheckmarkColor);
        toggle.graphic = checkImage;
        toggle.ungraphic = null;

        var labelObj = RowElementBuilder.CreateObject("Label", optionObj.transform);
        style.LabelRect.Apply(labelObj.GetComponent<RectTransform>());
        var labelText = RowElementBuilder.AddText(labelObj, style.LabelTextAppearance, label);
        labelText.raycastTarget = false;

        return toggle;
    }
}
