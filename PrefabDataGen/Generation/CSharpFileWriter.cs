namespace EmberConfig.PrefabDataGen.Generation;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EmberConfig.PrefabDataGen.Models;

internal static class CSharpFileWriter
{
    internal static void WriteRowStyleFactory(string outputPath, RowRawStyle row)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "RowStyleFactory", new[]
        {
            "Generated factory that creates the runtime RowStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "RowStyle", "TextMeshProUGUI? descriptionText");
        sb.AppendLine($"        var title = {TextAppearanceLiteral(row.TitleText)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "RowStyle", new List<string>
        {
            "title",
            $"SpriteResolver.Resolve({StringLiteral(row.BackgroundSpriteName)})",
            ColorLiteral(row.BackgroundColor),
            ColorLiteral(row.HighlightColor),
            $"(Image.Type){row.BackgroundType}",
            $"{Float(row.Height)}f",
            $"{Float(row.Width)}f",
            $"{Float(row.TitleWidth)}f",
            $"{Float(row.ItemWidth)}f",
            "descriptionText",
            RectDataLiteral(row.RowRect),
            RectDataLiteral(row.TitleRect),
            RectDataLiteral(row.ItemRect)
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    internal static void WriteDropdownStyleFactory(string outputPath, DropdownRawStyle dropdown)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "DropdownStyleFactory", new[]
        {
            "Generated factory that creates the runtime DropdownStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "DropdownStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var itemLabel = {TextAppearanceLiteral(dropdown.Item.LabelText)};");
        sb.AppendLine($"        var listItemLabel = {TextAppearanceLiteral(dropdown.Template.ItemLabelText)};");

        CSharpCodeBuilder.AppendRecordConstructor(sb, "item", "DropdownItemStyle", new List<string>
        {
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Item.ItemSpriteName)})",
            ColorLiteral(dropdown.Item.ItemColor),
            $"(Image.Type){dropdown.Item.ItemType}",
            RectDataLiteral(dropdown.Item.ItemRect),
            RectDataLiteral(dropdown.Item.LabelRect),
            "itemLabel",
            $"(TextAlignmentOptions){dropdown.Item.LabelAlignment}",
            RectDataLiteral(dropdown.Item.ArrowRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Item.ArrowSpriteName)})",
            ColorLiteral(dropdown.Item.ArrowColor),
            $"(Image.Type){dropdown.Item.ArrowType}",
            $"{dropdown.Item.ControllerKey}"
        });

        List<string>? templateInitializer = null;
        if (dropdown.Template.ItemColorBlock is not null)
        {
            var cb = dropdown.Template.ItemColorBlock;
            templateInitializer = new List<string>
            {
                "            ItemColorBlock = new ColorBlock",
                "            {",
                $"                normalColor = {ColorLiteral(cb.NormalColor)},",
                $"                highlightedColor = {ColorLiteral(cb.HighlightedColor)},",
                $"                pressedColor = {ColorLiteral(cb.PressedColor)},",
                $"                disabledColor = {ColorLiteral(cb.DisabledColor)},",
                $"                colorMultiplier = {Float(cb.ColorMultiplier)}f,",
                $"                fadeDuration = {Float(cb.FadeDuration)}f",
                "            }"
            };
        }

        CSharpCodeBuilder.AppendRecordConstructor(sb, "template", "DropdownTemplateStyle", new List<string>
        {
            RectDataLiteral(dropdown.Template.TemplateRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Template.TemplateSpriteName)})",
            ColorLiteral(dropdown.Template.TemplateBgColor),
            $"(Image.Type){dropdown.Template.TemplateImageType}",
            RectDataLiteral(dropdown.Template.ViewportRect),
            RectDataLiteral(dropdown.Template.ContentRect),
            RectDataLiteral(dropdown.Template.TemplateItemRect),
            RectDataLiteral(dropdown.Template.TemplateHighlightRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Template.TemplateHighlightSpriteName)})",
            ColorLiteral(dropdown.Template.TemplateHighlightColor),
            $"(Image.Type){dropdown.Template.TemplateHighlightType}",
            RectDataLiteral(dropdown.Template.ItemBackgroundRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Template.ItemBgSpriteName)})",
            ColorLiteral(dropdown.Template.ItemBgColor),
            $"(Image.Type){dropdown.Template.ItemBgType}",
            RectDataLiteral(dropdown.Template.ItemCheckmarkRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Template.ItemCheckmarkSpriteName)})",
            ColorLiteral(dropdown.Template.ItemCheckmarkColor),
            $"(Image.Type){dropdown.Template.ItemCheckmarkType}",
            RectDataLiteral(dropdown.Template.ItemLabelRect),
            "listItemLabel",
            $"(TextAlignmentOptions){dropdown.Template.ItemLabelAlignment}",
            $"{dropdown.Template.CtrlBackKey}"
        }, templateInitializer);

        CSharpCodeBuilder.AppendRecordConstructor(sb, "scrollbar", "DropdownScrollbarStyle", new List<string>
        {
            RectDataLiteral(dropdown.Scrollbar.ScrollbarRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Scrollbar.ScrollbarSpriteName)})",
            ColorLiteral(dropdown.Scrollbar.ScrollbarColor),
            $"(Image.Type){dropdown.Scrollbar.ScrollbarType}",
            RectDataLiteral(dropdown.Scrollbar.SlidingAreaRect),
            RectDataLiteral(dropdown.Scrollbar.HandleRect),
            $"SpriteResolver.Resolve({StringLiteral(dropdown.Scrollbar.HandleSpriteName)})",
            ColorLiteral(dropdown.Scrollbar.HandleColor),
            $"(Image.Type){dropdown.Scrollbar.HandleType}"
        });

        sb.AppendLine("        return new DropdownStyle(item, template, scrollbar);");
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    internal static void WriteSwitchStyleFactory(string outputPath, SwitchRawStyle raw)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "SwitchStyleFactory", new[]
        {
            "Generated factory that creates the runtime SwitchStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "SwitchStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var labelText = {TextAppearanceLiteral(raw.Option.LabelText)};");
        sb.AppendLine($"        var clickGroupLayout = {SwitchLayoutGroupLiteral(raw.ClickGroupLayout)};");
        sb.AppendLine($"        var optionColorBlock = {ColorBlockLiteral(raw.Option.OptionColorBlock)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "SwitchStyle", new List<string>
        {
            ColorLiteral(raw.Option.BackgroundColor),
            ColorLiteral(raw.Option.CheckmarkColor),
            $"SpriteResolver.Resolve({StringLiteral(raw.Option.BackgroundSpriteName)})",
            $"SpriteResolver.Resolve({StringLiteral(raw.Option.CheckmarkSpriteName)})",
            $"(Image.Type){raw.Option.BackgroundType}",
            $"(Image.Type){raw.Option.CheckmarkType}",
            "labelText",
            RectDataLiteral(raw.ClickGroupRect),
            RectDataLiteral(raw.Option.OptionRect),
            RectDataLiteral(raw.Option.LabelRect),
            RectDataLiteral(raw.Option.BackgroundRect),
            RectDataLiteral(raw.Option.CheckmarkRect),
            "clickGroupLayout",
            "optionColorBlock",
            $"(Selectable.Transition){raw.Option.Transition}",
            Bool(raw.AllowSwitchOff),
            $"{raw.ClickSoundEventId}u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    internal static void WriteKeybindButtonStyleFactory(string outputPath, KeybindButtonRawStyle raw)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "KeybindButtonStyleFactory", new[]
        {
            "Generated factory that creates the runtime KeybindButtonStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "KeybindButtonStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var text = {TextAppearanceLiteral(raw.Text)};");
        sb.AppendLine($"        var noneText = {TextAppearanceLiteral(raw.NoneText)};");
        sb.AppendLine($"        var buttonColorBlock = {ColorBlockLiteral(raw.ButtonColorBlock)};");
        sb.AppendLine($"        var itemLayout = {KeybindItemLayoutLiteral(raw.ItemLayout)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "KeybindButtonStyle", new List<string>
        {
            "text",
            "noneText",
            $"TMP_SpriteAssetResolver.Resolve({StringLiteral(raw.SpriteAssetName)})",
            ColorLiteral(raw.BackgroundColor),
            $"SpriteResolver.Resolve({StringLiteral(raw.BackgroundSpriteName)})",
            $"(Image.Type){raw.BackgroundType}",
            "buttonColorBlock",
            $"(Selectable.Transition){raw.ButtonTransition}",
            RectDataLiteral(raw.PrimaryRect),
            RectDataLiteral(raw.SecondaryRect),
            RectDataLiteral(raw.ItemRect),
            "itemLayout"
        }, new List<string>
        {
            $"            ClickSoundEventId = {raw.ClickSoundEventId}u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string KeybindItemLayoutLiteral(KeybindItemLayoutRawStyle layout) =>
        $"new KeybindItemLayout({Float(layout.Spacing)}f, (TextAnchor){layout.ChildAlignment}, {layout.PaddingLeft}, {layout.PaddingRight}, {layout.PaddingTop}, {layout.PaddingBottom}, {Bool(layout.ChildControlWidth)}, {Bool(layout.ChildControlHeight)}, {Bool(layout.ChildForceExpandWidth)}, {Bool(layout.ChildForceExpandHeight)})";

    internal static void WriteCarouselStyleFactory(string outputPath, CarouselRawStyle raw)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "CarouselStyleFactory", new[]
        {
            "Generated factory that creates the runtime CarouselStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "CarouselStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var valueText = {TextAppearanceLiteral(raw.ValueText)};");
        sb.AppendLine($"        var arrowButtonColorBlock = {ColorBlockLiteral(raw.ArrowButtonColorBlock)};");
        sb.AppendLine($"        var dotGroupLayout = {DotGroupLayoutLiteral(raw.DotGroupLayout)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "CarouselStyle", new List<string>
        {
            "valueText",
            "arrowButtonColorBlock",
            $"(Selectable.Transition){raw.ArrowButtonTransition}",
            $"SpriteResolver.Resolve({StringLiteral(raw.ArrowImageSpriteName)})",
            ColorLiteral(raw.ArrowImageColor),
            $"(Image.Type){raw.ArrowImageType}",
            RectDataLiteral(raw.ArrowImageRect),
            RectDataLiteral(raw.NextArrowImageRect),
            RectDataLiteral(raw.ItemRect),
            RectDataLiteral(raw.MutiClickGroupRect),
            RectDataLiteral(raw.PreviousButtonRect),
            RectDataLiteral(raw.NextButtonRect),
            RectDataLiteral(raw.SettingInfoRect),
            RectDataLiteral(raw.NowsetionRect),
            RectDataLiteral(raw.DotGroupRect),
            RectDataLiteral(raw.DotRootRect),
            RectDataLiteral(raw.DotChildRect),
            ColorLiteral(raw.DotBackgroundColor),
            ColorLiteral(raw.DotCheckmarkColor),
            $"SpriteResolver.Resolve({StringLiteral(raw.DotSpriteName)})",
            $"(Image.Type){raw.DotType}",
            "dotGroupLayout"
        }, new List<string>
        {
            $"            ClickSoundEventId = {raw.ClickSoundEventId}u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    internal static void WriteInputStyleFactory(string outputPath)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "InputStyleFactory", new[]
        {
            "Generated factory that creates the runtime InputStyle from row style fallbacks.",
            "PC_Panel_setting does not contain a native input field, so this style is hand-tuned to match the settings row look."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "InputStyle", "Sprite? fallbackSprite, TextAppearance fallbackText");
        sb.AppendLine("        var text = fallbackText with { Alignment = TextAlignmentOptions.Center };");
        sb.AppendLine("        var placeholderColor = new Color(text.Color.r, text.Color.g, text.Color.b, 0.5f);");
        sb.AppendLine("        var placeholder = text with { Color = placeholderColor, FontStyle = FontStyles.Italic };");
        sb.AppendLine("        var inputRect = new RectData(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(277.5f, 31f), new Vector2(-51.25f, 0f), new Vector2(1f, 0.5f));");
        sb.AppendLine("        var textAreaRect = new RectData(Vector2.zero, Vector2.one, new Vector2(-20f, -8f), Vector2.zero, new Vector2(0.5f, 0.5f));");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "InputStyle", new List<string>
        {
            "new Color(1f, 1f, 1f, 1f)",
            "SpriteResolver.Resolve(\"bar_bg_unlock\") ?? fallbackSprite",
            "Image.Type.Simple",
            "inputRect",
            "textAreaRect",
            "text",
            "placeholder",
            "InputStyle.DefaultSelectionColor"
        }, new List<string>
        {
            "            ClickSoundEventId = 0u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string DotGroupLayoutLiteral(DotGroupLayoutRawStyle layout) =>
        $"new DotGroupLayout({Float(layout.Spacing)}f, (TextAnchor){layout.ChildAlignment}, {layout.PaddingLeft}, {layout.PaddingRight}, {layout.PaddingTop}, {layout.PaddingBottom}, {Bool(layout.ChildControlWidth)}, {Bool(layout.ChildControlHeight)}, {Bool(layout.ChildForceExpandWidth)}, {Bool(layout.ChildForceExpandHeight)})";

    internal static void WriteTabStyleFactory(string outputPath, TabRawStyle raw)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "TabStyleFactory", new[]
        {
            "Generated factory that creates the runtime TabStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "TabStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var selected = {TextAppearanceLiteral(raw.Selected)};");
        sb.AppendLine($"        var unselected = {TextAppearanceLiteral(raw.Unselected)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "TabStyle", new List<string>
        {
            "selected",
            "unselected",
            $"{Float(raw.Width)}f",
            $"{Float(raw.Height)}f"
        }, new List<string>
        {
            $"            SelectedBackgroundSprite = SpriteResolver.Resolve({StringLiteral(raw.SelectedBackgroundSpriteName)}),",
            $"            SelectedBackgroundRect = {RectDataLiteral(raw.SelectedBackgroundRect)},",
            $"            ClickSoundEventId = {raw.ClickSoundEventId}u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    internal static void WriteSliderStyleFactory(string outputPath, SliderRawStyle raw)
    {
        var sb = new StringBuilder();
        CSharpCodeBuilder.AppendFileHeader(sb, "SliderStyleFactory", new[]
        {
            "Generated factory that creates the runtime SliderStyle from raw prefab data."
        });
        CSharpCodeBuilder.AppendMethodStart(sb, "SliderStyle", "TextAppearance fallbackText");
        sb.AppendLine($"        var numText = {TextAppearanceLiteral(raw.NumText)};");
        sb.AppendLine($"        var sliderColorBlock = {ColorBlockLiteral(raw.SliderColorBlock)};");
        CSharpCodeBuilder.AppendRecordConstructor(sb, null, "SliderStyle", new List<string>
        {
            ColorLiteral(raw.BackgroundColor),
            ColorLiteral(raw.BgColor),
            ColorLiteral(raw.FillColor),
            ColorLiteral(raw.HandleColor),
            $"SpriteResolver.Resolve({StringLiteral(raw.BackgroundSpriteName)})",
            $"SpriteResolver.Resolve({StringLiteral(raw.FillSpriteName)})",
            $"SpriteResolver.Resolve({StringLiteral(raw.HandleSpriteName)})",
            $"(Image.Type){raw.FillImageType}",
            $"(Image.FillMethod){raw.FillFillMethod}",
            RectDataLiteral(raw.SliderPcUnitRect),
            RectDataLiteral(raw.SliderRect),
            RectDataLiteral(raw.BackgroundRect),
            RectDataLiteral(raw.BgRect),
            RectDataLiteral(raw.FillAreaRect),
            RectDataLiteral(raw.FillRect),
            RectDataLiteral(raw.HandleSlideAreaRect),
            RectDataLiteral(raw.HandleRect),
            RectDataLiteral(raw.NumRect),
            "numText",
            $"{Float(raw.Spacing)}f",
            $"(TextAnchor){raw.ChildAlignment}",
            $"{raw.PaddingLeft}",
            $"{raw.PaddingRight}",
            $"{raw.PaddingTop}",
            $"{raw.PaddingBottom}",
            Bool(raw.ChildControlWidth),
            Bool(raw.ChildControlHeight),
            Bool(raw.ChildForceExpandWidth),
            Bool(raw.ChildForceExpandHeight),
            "sliderColorBlock",
            $"(Selectable.Transition){raw.SliderTransition}",
            $"(Slider.Direction){raw.Direction}",
            Bool(raw.WholeNumbers),
            $"{Float(raw.MinValue)}f",
            $"{Float(raw.MaxValue)}f"
        }, new List<string>
        {
            $"            ClickSoundEventId = {raw.ClickSoundEventId}u"
        });
        CSharpCodeBuilder.AppendMethodEnd(sb);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string ColorBlockLiteral(ExtractedColorBlock cb) =>
        $"new ColorBlock {{ normalColor = {ColorLiteral(cb.NormalColor)}, highlightedColor = {ColorLiteral(cb.HighlightedColor)}, pressedColor = {ColorLiteral(cb.PressedColor)}, disabledColor = {ColorLiteral(cb.DisabledColor)}, colorMultiplier = {Float(cb.ColorMultiplier)}f, fadeDuration = {Float(cb.FadeDuration)}f }}";

    private static string SwitchLayoutGroupLiteral(SwitchLayoutRawStyle layout) =>
        $"new SwitchLayoutGroup({Float(layout.Spacing)}f, (TextAnchor){layout.ChildAlignment}, {Bool(layout.ChildControlWidth)}, {Bool(layout.ChildControlHeight)}, {Bool(layout.ChildForceExpandWidth)}, {Bool(layout.ChildForceExpandHeight)}, {layout.PaddingLeft}, {layout.PaddingRight}, {layout.PaddingTop}, {layout.PaddingBottom})";

    private static string ColorLiteral(Color color) =>
        $"new Color({Float(color.R)}f, {Float(color.G)}f, {Float(color.B)}f, {Float(color.A)}f)";

    private static string RectDataLiteral(RectData rect) =>
        $"new RectData(new Vector2({Float(rect.AnchorMinX)}f, {Float(rect.AnchorMinY)}f), new Vector2({Float(rect.AnchorMaxX)}f, {Float(rect.AnchorMaxY)}f), new Vector2({Float(rect.SizeDeltaX)}f, {Float(rect.SizeDeltaY)}f), new Vector2({Float(rect.AnchoredPositionX)}f, {Float(rect.AnchoredPositionY)}f), new Vector2({Float(rect.PivotX)}f, {Float(rect.PivotY)}f))";

    private static string TextAppearanceLiteral(ExtractedTextAppearance text) =>
        $"new TextAppearance(TMP_FontAssetResolver.Resolve({StringLiteral(text.FontAssetName)}), MaterialResolver.Resolve({StringLiteral(text.MaterialName)}), {Float(text.FontSize)}f, {ColorLiteral(text.Color)}, (TextAlignmentOptions){text.Alignment}, (FontStyles){text.FontStyle}, {Float(text.OutlineWidth)}f, {Bool(text.EnableWordWrapping)}, {Bool(text.EnableAutoSizing)}, (TextOverflowModes){text.OverflowMode}, {Float(text.FontSizeMin)}f, {Float(text.FontSizeMax)}f)";

    private static string StringLiteral(string? value) =>
        value is null ? "null" : $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Bool(bool value) => value ? "true" : "false";

    private static string Float(float value) =>
        value.ToString(CultureInfo.InvariantCulture);
}
