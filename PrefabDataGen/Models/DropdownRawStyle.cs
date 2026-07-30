namespace EmberConfig.PrefabDataGen.Models;

internal sealed record DropdownRawStyle(
    DropdownItemRawStyle Item,
    DropdownTemplateRawStyle Template,
    DropdownScrollbarRawStyle Scrollbar);

internal sealed record DropdownItemRawStyle(
    string? ItemSpriteName,
    Color ItemColor,
    int ItemType,
    RectData ItemRect,
    RectData LabelRect,
    ExtractedTextAppearance LabelText,
    int LabelAlignment,
    RectData ArrowRect,
    string? ArrowSpriteName,
    Color ArrowColor,
    int ArrowType,
    int ControllerKey);

internal sealed record DropdownTemplateRawStyle(
    RectData TemplateRect,
    string? TemplateSpriteName,
    Color TemplateBgColor,
    int TemplateImageType,
    RectData ViewportRect,
    RectData ContentRect,
    RectData TemplateItemRect,
    RectData TemplateHighlightRect,
    string? TemplateHighlightSpriteName,
    Color TemplateHighlightColor,
    int TemplateHighlightType,
    RectData ItemBackgroundRect,
    string? ItemBgSpriteName,
    Color ItemBgColor,
    int ItemBgType,
    RectData ItemCheckmarkRect,
    string? ItemCheckmarkSpriteName,
    Color ItemCheckmarkColor,
    int ItemCheckmarkType,
    RectData ItemLabelRect,
    ExtractedTextAppearance ItemLabelText,
    int ItemLabelAlignment,
    int CtrlBackKey,
    ExtractedColorBlock? ItemColorBlock = null);

internal sealed record DropdownScrollbarRawStyle(
    RectData ScrollbarRect,
    string? ScrollbarSpriteName,
    Color ScrollbarColor,
    int ScrollbarType,
    RectData SlidingAreaRect,
    RectData HandleRect,
    string? HandleSpriteName,
    Color HandleColor,
    int HandleType);
