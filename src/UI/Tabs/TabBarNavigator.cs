namespace SettingsLib.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class TabBarNavigator
{
    private TabButtonCollection? buttons;
    private TabStyle? style;
    private M1ToggleGroup? arrowGroup;
    private Action<int>? selectTab;

    public void Initialize(TabButtonCollection buttons, TabStyle style, Action<int> selectTab)
    {
        this.buttons = buttons ?? throw new ArgumentNullException(nameof(buttons));
        this.style = style;
        this.selectTab = selectTab ?? throw new ArgumentNullException(nameof(selectTab));
    }

    public void AttachArrowButtons(M1ToggleGroup? group)
    {
        DetachArrowButtons();
        arrowGroup = group;
        if (arrowGroup is null)
            return;

        if (arrowGroup.m_Left is not null)
        {
            arrowGroup.m_Left.onClick.RemoveAllListeners();
            Action leftClick = () => NavigatePrevious();
            arrowGroup.m_Left.onClick.AddListener(leftClick);
        }

        if (arrowGroup.m_Right is not null)
        {
            arrowGroup.m_Right.onClick.RemoveAllListeners();
            Action rightClick = () => NavigateNext();
            arrowGroup.m_Right.onClick.AddListener(rightClick);
        }
    }

    public void DetachArrowButtons()
    {
        if (arrowGroup is null)
            return;

        arrowGroup.m_Left?.onClick.RemoveAllListeners();
        arrowGroup.m_Right?.onClick.RemoveAllListeners();
        arrowGroup = null;
    }

    public void Reset() => DetachArrowButtons();

    public void NavigateNext()
    {
        if (buttons is null || buttons.Count == 0)
            return;

        var activeIndex = buttons.GetActiveIndex();
        if (activeIndex >= 0)
            WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, buttons.Toggles[activeIndex].gameObject);

        var start = activeIndex >= 0 ? activeIndex : -1;
        selectTab?.Invoke(TabBarLayout.Mod(start + 1, buttons.Count));
    }

    public void NavigatePrevious()
    {
        if (buttons is null || buttons.Count == 0)
            return;

        var activeIndex = buttons.GetActiveIndex();
        if (activeIndex >= 0)
            WwiseAudio.PostIfValid(style?.ClickSoundEventId ?? 0u, buttons.Toggles[activeIndex].gameObject);

        var start = activeIndex >= 0 ? activeIndex : 0;
        selectTab?.Invoke(TabBarLayout.Mod(start - 1, buttons.Count));
    }
}
