namespace SettingsLib.UI;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal readonly record struct TabButton(
    M1Toggle Toggle,
    GameObject? Background,
    TextMeshProUGUI? Label);

internal sealed class TabButtonCollection
{
    private readonly NativeTabResolver nativeResolver;
    private readonly List<TabButton> buttons = new();
    private readonly List<M1Toggle> toggles = new();

    private TabStyle? style;
    private bool nativeTabsBuilt;
    private int initialActiveIndex = -1;
    private bool isNotifying;
    private RectTransform? content;

    public TabButtonCollection(NativeTabResolver nativeResolver)
    {
        this.nativeResolver = nativeResolver ?? throw new ArgumentNullException(nameof(nativeResolver));
    }

    public IReadOnlyList<M1Toggle> Toggles => toggles;
    public IReadOnlyList<TabButton> Buttons => buttons;
    public int Count => buttons.Count;
    public int InitialActiveIndex => initialActiveIndex;

    public event Action<int>? TabSelected;

    public void Initialize(RectTransform? content)
    {
        this.content = content;
    }

    public void BuildNativeTabs(IReadOnlyList<NativeTabResolver.NativeTabInfo> infos, RectTransform? content, TabStyle? style)
    {
        if (nativeTabsBuilt || content is null || !style.HasValue)
            return;

        this.style = style;

        for (int i = 0; i < infos.Count; i++)
        {
            var info = infos[i];
            var toggle = TabButtonBuilder.Build($"tab_native_{info.ContentName}", info.Label, style.Value, content);
            toggle.transform.SetSiblingIndex(i);
            nativeResolver.Register(toggle, info.ContentName);

            if (info.IsActive)
                initialActiveIndex = i;
        }

        nativeTabsBuilt = true;
    }

    public void RebuildFromContent(RectTransform? content)
    {
        buttons.Clear();
        toggles.Clear();

        if (content is null)
            return;

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            if (child is null)
                continue;

            var toggle = child.GetComponent<M1Toggle>();
            if (toggle is null)
                continue;

            var background = child.Find("Background")?.gameObject;
            var label = child.Find("type_name")?.GetComponent<TextMeshProUGUI>();
            buttons.Add(new TabButton(toggle, background, label));
            toggles.Add(toggle);
        }

        for (int i = 0; i < toggles.Count; i++)
        {
            var capturedToggle = toggles[i];
            var capturedIndex = i;

            capturedToggle.onValueChanged.RemoveAllListeners();
            Action<bool> onToggled = isOn =>
            {
                if (isNotifying)
                    return;

                if (isOn)
                    WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, capturedToggle.gameObject);

                SelectTab(capturedIndex);
            };
            capturedToggle.onValueChanged.AddListener(onToggled);
        }
    }

    public M1Toggle? GetActiveToggle()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
                return toggles[i];
        }

        return null;
    }

    public int GetActiveIndex()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
                return i;
        }

        return -1;
    }

    public int GetIndex(M1Toggle? toggle)
    {
        if (toggle is null)
            return -1;

        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i] == toggle)
                return i;
        }

        return -1;
    }

    public M1Toggle? GetToggle(int index) =>
        index >= 0 && index < toggles.Count ? toggles[index] : null;

    public void SelectTab(int index)
    {
        if (toggles.Count == 0)
            return;

        index = TabBarLayout.Mod(index, toggles.Count);

        isNotifying = true;
        for (int i = 0; i < toggles.Count; i++)
            toggles[i].SetIsOnWithoutNotify(i == index);
        isNotifying = false;

        TabSelected?.Invoke(index);
    }

    public void SetInitialActive(int index)
    {
        if (index < 0 || index >= toggles.Count)
            return;

        toggles[index].SetIsOnWithoutNotify(true);
    }

    public void Clear()
    {
        buttons.Clear();
        toggles.Clear();
        style = null;
        nativeTabsBuilt = false;
        initialActiveIndex = -1;
        content = null;
    }
}
