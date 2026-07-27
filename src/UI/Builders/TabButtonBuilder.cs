namespace SettingsLib.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class TabButtonBuilder
{
    internal static M1Toggle Build(string tabName, TabStyle tabStyle, M1ToggleGroup group, Action<string> onActivated)
    {
        Plugin.Logger?.LogInfo($"TabButtonBuilder.Build: tabName={tabName} style sprite={tabStyle.SelectedBackgroundSprite?.name ?? "null"} width={tabStyle.Width} height={tabStyle.Height}");
        var go = new GameObject($"tab_custom_{tabName}");
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(group.transform, false);
        RowElementBuilder.SetRect(rect, Vector2.zero, Vector2.zero, new Vector2(tabStyle.Width, tabStyle.Height), Vector2.zero);

        var toggle = go.AddComponent<M1Toggle>();
        go.AddComponent<CanvasRenderer>();

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
        var typeNameText = RowElementBuilder.AddText(typeNameObj, tabStyle.Unselected, tabName, TextAlignmentOptions.Center);
        typeNameText.raycastTarget = true;

        toggle.targetGraphic = null;
        toggle.graphic = null;
        toggle.ungraphic = null;
        toggle.transition = Selectable.Transition.None;
        toggle.interactable = true;

        toggle.m_Group = group;
        group.RegisterToggle(toggle);

        backgroundObj.SetActive(false);

        System.Action<bool> onToggled = isOn =>
        {
            backgroundObj.SetActive(isOn);
            typeNameText.color = isOn ? tabStyle.Selected.Color : tabStyle.Unselected.Color;
            if (!isOn)
                return;

            WwiseAudio.PostIfValid(tabStyle.ClickSoundEventId, go);

            onActivated(tabName);
        };
        toggle.onValueChanged.AddListener(onToggled);

        toggle.SetIsOnWithoutNotify(false);

        VanillaComponentApplier.AttachAudio(go.transform);

        go.SetActive(true);
        return toggle;
    }
}
