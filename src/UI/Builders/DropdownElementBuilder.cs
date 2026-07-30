namespace EmberConfig.UI;

using DYControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class DropdownElementBuilder
{
    internal static Transform Build(string name, RowStyle style, DropdownStyle dropdownStyle, Transform parent)
    {
        var item = dropdownStyle.Item;
        var template = dropdownStyle.Template;
        var scrollbar = dropdownStyle.Scrollbar;

        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var rowItem = RowElementBuilder.CreateItem(style, root);
        var dropdownObj = RowElementBuilder.CreateObject("Dropdown", rowItem.transform);
        dropdownObj.SetActive(false);
        item.ItemRect.Apply(dropdownObj.GetComponent<RectTransform>());
        var image = RowElementBuilder.AddImage(dropdownObj, item.ItemSprite, item.ItemType, item.ItemColor);

        var labelObj = RowElementBuilder.CreateObject("Label", dropdownObj.transform);
        item.LabelRect.Apply(labelObj.GetComponent<RectTransform>());
        var labelText = RowElementBuilder.AddText(labelObj, item.LabelTextAppearance, string.Empty, item.LabelAlignment);

        var arrowObj = RowElementBuilder.CreateObject("Arrow", dropdownObj.transform);
        item.ArrowRect.Apply(arrowObj.GetComponent<RectTransform>());
        var arrowImage = RowElementBuilder.AddImage(arrowObj, item.ArrowSprite, item.ArrowType, item.ArrowColor);
        arrowImage.raycastTarget = false;

        var templateObj = RowElementBuilder.CreateObject("Template", dropdownObj.transform);
        template.TemplateRect.Apply(templateObj.GetComponent<RectTransform>());
        templateObj.SetActive(false);
        RowElementBuilder.AddImage(templateObj, template.TemplateSprite, template.TemplateImageType, template.TemplateBgColor);

        var scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var dyCtrlScrollRect = templateObj.AddComponent<DYCtrlDropDownScrollRect>();

        var viewportObj = RowElementBuilder.CreateObject("Viewport", templateObj.transform);
        template.ViewportRect.Apply(viewportObj.GetComponent<RectTransform>());
        var viewportImage = RowElementBuilder.AddImage(viewportObj, template.TemplateSprite, template.TemplateImageType, Color.white);
        var viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        scrollRect.viewport = viewportObj.GetComponent<RectTransform>();

        var contentObj = RowElementBuilder.CreateObject("Content", viewportObj.transform);
        template.ContentRect.Apply(contentObj.GetComponent<RectTransform>());
        scrollRect.content = contentObj.GetComponent<RectTransform>();

        var templateItemObj = RowElementBuilder.CreateObject("Item", contentObj.transform);
        template.TemplateItemRect.Apply(templateItemObj.GetComponent<RectTransform>());
        var templateItemToggle = templateItemObj.AddComponent<Toggle>();
        templateItemToggle.isOn = false;

        var templateHighlightObj = RowElementBuilder.CreateObject("Image", templateItemObj.transform);
        template.TemplateHighlightRect.Apply(templateHighlightObj.GetComponent<RectTransform>());
        var templateHighlightImage = RowElementBuilder.AddImage(templateHighlightObj, template.TemplateHighlightSprite, template.TemplateHighlightType, template.TemplateHighlightColor);

        var itemBgObj = RowElementBuilder.CreateObject("Item Background", templateItemObj.transform);
        template.ItemBackgroundRect.Apply(itemBgObj.GetComponent<RectTransform>());
        var itemBgImage = RowElementBuilder.AddImage(itemBgObj, template.ItemBgSprite, template.ItemBgType, template.ItemBgColor);
        templateItemToggle.targetGraphic = itemBgImage;
        templateItemToggle.colors = template.ItemColorBlock;

        var itemCheckObj = RowElementBuilder.CreateObject("Item Checkmark", templateItemObj.transform);
        template.ItemCheckmarkRect.Apply(itemCheckObj.GetComponent<RectTransform>());
        var itemCheckImage = RowElementBuilder.AddImage(itemCheckObj, template.ItemCheckmarkSprite, template.ItemCheckmarkType, template.ItemCheckmarkColor);
        templateItemToggle.graphic = itemCheckImage;

        var itemLabelObj = RowElementBuilder.CreateObject("Item Label", templateItemObj.transform);
        template.ItemLabelRect.Apply(itemLabelObj.GetComponent<RectTransform>());
        var itemLabelText = RowElementBuilder.AddText(itemLabelObj, template.ItemLabelTextAppearance, string.Empty, template.ItemLabelAlignment);
        itemLabelText.raycastTarget = true;

        var scrollbarObj = RowElementBuilder.CreateObject("Scrollbar", templateObj.transform);
        scrollbar.ScrollbarRect.Apply(scrollbarObj.GetComponent<RectTransform>());
        RowElementBuilder.AddImage(scrollbarObj, scrollbar.ScrollbarSprite, scrollbar.ScrollbarType, scrollbar.ScrollbarColor);
        var uiScrollbar = scrollbarObj.AddComponent<Scrollbar>();
        uiScrollbar.direction = Scrollbar.Direction.BottomToTop;

        var slidingAreaObj = RowElementBuilder.CreateObject("Sliding Area", scrollbarObj.transform);
        scrollbar.SlidingAreaRect.Apply(slidingAreaObj.GetComponent<RectTransform>());

        var handleObj = RowElementBuilder.CreateObject("Handle", slidingAreaObj.transform);
        scrollbar.HandleRect.Apply(handleObj.GetComponent<RectTransform>());
        var handleImage = RowElementBuilder.AddImage(handleObj, scrollbar.HandleSprite, scrollbar.HandleType, scrollbar.HandleColor);
        handleImage.raycastTarget = false;
        uiScrollbar.targetGraphic = handleImage;

        scrollRect.verticalScrollbar = uiScrollbar;

        var dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        dropdown.onValueChanged ??= new TMP_Dropdown.DropdownEvent();
        dropdown.targetGraphic = image;
        dropdown.captionText = labelText;
        dropdown.template = templateObj.GetComponent<RectTransform>();
        dropdown.itemText = itemLabelText;
        dropdown.itemImage = templateHighlightImage;
        dropdown.interactable = true;

        var controllerLink = dropdownObj.AddComponent<ControllerLinkToggle>();
        controllerLink.LinkedDropDown = dropdown;
        controllerLink.buttontext = labelText;
        if (item.ControllerKey != 0)
            controllerLink.ControllerKey = (ControllerConstKey)item.ControllerKey;

        dyCtrlScrollRect.dropdown = dropdown;
        dyCtrlScrollRect.srviewport = viewportObj.GetComponent<RectTransform>();
        dyCtrlScrollRect.srcontent = contentObj.GetComponent<RectTransform>();
        dyCtrlScrollRect.ctrlBackKey = (ControllerConstKey)template.CtrlBackKey;

        var listItemDySelect = templateItemObj.AddComponent<DYSelect>();
        listItemDySelect.unitySel = templateItemToggle;
        listItemDySelect.isCurBtn = true;
        listItemDySelect.defaultCanSelect = false;
        listItemDySelect.selectExcuteClick = false;
        listItemDySelect.curObj = dyCtrlScrollRect.gameObject;

        VanillaComponentApplier.ApplyToRow(root.transform, dropdown);
        var rootDySelect = root.GetComponent<DYSelect>();
        if (rootDySelect is not null)
        {
            dyCtrlScrollRect.backSel = rootDySelect;
            rootDySelect.isCurBtn = true;
        }

        VanillaComponentApplier.AttachAudio(dropdownObj.transform, true);
        VanillaComponentApplier.AttachAudio(templateItemObj.transform, true);

        dropdownObj.SetActive(true);

        return root.transform;
    }
}
