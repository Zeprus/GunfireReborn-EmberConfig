namespace SettingsLib.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class InputElementBuilder
{
    internal static Transform Build(string name, RowStyle style, InputStyle? inputStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var item = RowElementBuilder.CreateItem(style, root);
        var inputObj = RowElementBuilder.CreateObject("InputField", item.transform);
        var inputRect = inputStyle?.InputRect ?? new RectData(
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, RowElementBuilder.Metrics.ControlHeight),
            Vector2.zero, new Vector2(0.5f, 0.5f));
        inputRect.Apply(inputObj.GetComponent<RectTransform>());

        var image = RowElementBuilder.AddImage(
            inputObj,
            inputStyle?.BackgroundSprite ?? style.BackgroundSprite,
            inputStyle?.BackgroundType ?? Image.Type.Sliced,
            inputStyle?.BackgroundColor ?? new Color(1f, 1f, 1f, 0.1f));
        inputObj.AddComponent<CanvasRenderer>();

        var textAreaObj = RowElementBuilder.CreateObject("Text Area", inputObj.transform);
        var textAreaRect = inputStyle?.TextAreaRect ?? InputStyle.DefaultTextAreaRect;
        textAreaRect.Apply(textAreaObj.GetComponent<RectTransform>());
        textAreaObj.AddComponent<RectMask2D>();

        var textObj = RowElementBuilder.CreateObject("Text", textAreaObj.transform);
        RowElementBuilder.SetRect(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var text = RowElementBuilder.AddText(
            textObj,
            inputStyle?.TextAppearance ?? style.Title,
            string.Empty,
            TextAlignmentOptions.Left);

        var placeholderObj = RowElementBuilder.CreateObject("Placeholder", textAreaObj.transform);
        RowElementBuilder.SetRect(placeholderObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var placeholder = RowElementBuilder.AddText(
            placeholderObj,
            inputStyle?.PlaceholderAppearance ?? style.Title,
            "...",
            TextAlignmentOptions.Left);
        placeholder.raycastTarget = false;

        var input = inputObj.AddComponent<TMP_InputField>();
        input.textViewport = textAreaObj.GetComponent<RectTransform>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.targetGraphic = image;
        input.pointSize = (inputStyle?.TextAppearance ?? style.Title).FontSize;
        input.interactable = true;

        VanillaComponentApplier.ApplyToRow(root.transform, input);
        VanillaComponentApplier.AttachAudio(input.transform);

        return root.transform;
    }
}
