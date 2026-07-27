namespace SettingsLib.UI;

using UnityEngine;
using UnityEngine.UI;

internal static class ToggleElementBuilder
{
    internal static Transform Build(string name, RowStyle style, ToggleStyle toggleStyle, Transform parent)
    {
        var root = RowElementBuilder.CreateRowRoot(name, style, parent);
        RowElementBuilder.CreateTitle(style, root);

        var item = RowElementBuilder.CreateItem(style, root);
        var toggleObj = RowElementBuilder.CreateObject("Toggle", item.transform);
        RowElementBuilder.SetRect(toggleObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(RowElementBuilder.Metrics.ControlHeight, RowElementBuilder.Metrics.ControlHeight), new Vector2(20f, 0f));

        var toggle = toggleObj.AddComponent<M1Toggle>();
        toggleObj.AddComponent<CanvasRenderer>();

        var bgObj = RowElementBuilder.CreateObject("Background", toggleObj.transform);
        RowElementBuilder.SetRect(bgObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RowElementBuilder.AddImage(bgObj, toggleStyle.BackgroundSprite, Image.Type.Sliced, toggleStyle.BackgroundColor);

        var checkObj = RowElementBuilder.CreateObject("Checkmark", toggleObj.transform);
        RowElementBuilder.SetRect(checkObj, Vector2.zero, Vector2.one, new Vector2(-4f, -4f), Vector2.zero);
        RowElementBuilder.AddImage(checkObj, toggleStyle.CheckmarkSprite, Image.Type.Simple, toggleStyle.CheckmarkColor);

        toggle.targetGraphic = bgObj.GetComponent<Image>();
        toggle.graphic = checkObj.GetComponent<Image>();
        toggle.ungraphic = null;
        toggle.interactable = true;

        VanillaComponentApplier.ApplyToRow(root.transform, toggle);
        VanillaComponentApplier.AttachAudio(toggle.transform);

        return root.transform;
    }
}
