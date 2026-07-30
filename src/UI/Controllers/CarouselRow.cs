namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class CarouselRow : SettingRowBase
{
    private readonly CarouselStyle carouselStyle;
    private readonly uint clickSoundEventId;
    private readonly List<Image> dots = new();

    private TextMeshProUGUI? titleText;
    private TextMeshProUGUI? valueText;
    private M1Button? previousButton;
    private M1Button? nextButton;
    private Transform? dotGroup;

    private object?[] options = Array.Empty<object?>();
    private int currentIndex;
    private Action? onPreviousClicked;
    private Action? onNextClicked;

    public CarouselRow(Transform transform, CarouselStyle carouselStyle, uint clickSoundEventId) : base(transform)
    {
        this.carouselStyle = carouselStyle;
        this.clickSoundEventId = clickSoundEventId;
    }

    protected override void OnBind(ISettingEntry entry)
    {
        titleText = FindTitleText();
        valueText = Transform.Find("Item/MutiClickGroup/setting_info/nowsetion/Text")?.GetComponent<TextMeshProUGUI>();
        previousButton = Transform.Find("Item/MutiClickGroup/previous")?.GetComponent<M1Button>();
        nextButton = Transform.Find("Item/MutiClickGroup/next")?.GetComponent<M1Button>();
        dotGroup = Transform.Find("Item/MutiClickGroup/setting_info/Toggle_group");

        options = OptionResolver.Resolve(entry);

        BuildDots(options.Length);

        if (previousButton is not null)
        {
            onPreviousClicked = () => Navigate(-1);
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(onPreviousClicked);
        }

        if (nextButton is not null)
        {
            onNextClicked = () => Navigate(1);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(onNextClicked);
        }
    }

    private void BuildDots(int count)
    {
        if (dotGroup is null || count <= 0)
            return;

        for (int i = dotGroup.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(dotGroup.GetChild(i).gameObject);

        dots.Clear();

        for (int i = 0; i < count; i++)
        {
            var dot = CarouselElementBuilder.CreateDot(carouselStyle, i, dotGroup);
            dots.Add(dot.GetComponent<Image>());
        }
    }

    protected override void OnRefresh()
    {
        SafeSetText(titleText, Entry?.Label ?? string.Empty);

        if (Entry is null)
            return;

        currentIndex = FindCurrentIndex();
        UpdateValueText();
        UpdateDots();
    }

    private int FindCurrentIndex()
    {
        var current = Entry?.Config.BoxedValue;
        if (current is null)
            return 0;

        int index = Array.IndexOf(options, current);
        if (index < 0)
            index = OptionResolver.FindIndexByDisplayName(options, current);

        return Math.Max(0, index);
    }

    private void Navigate(int direction)
    {
        WwiseAudio.PostIfValid(clickSoundEventId, GameObject);

        if (options.Length == 0)
            return;

        var next = currentIndex + direction;
        if (next < 0)
            next = options.Length - 1;
        else if (next >= options.Length)
            next = 0;

        currentIndex = next;
        if (Entry is not null && currentIndex < options.Length)
        {
            Entry.Config.BoxedValue = options[currentIndex];
            Entry.Config.ConfigFile.Save();
        }

        UpdateValueText();
        UpdateDots();
    }

    private void UpdateValueText()
    {
        if (valueText is null || options.Length == 0)
            return;

        SafeSetText(valueText, currentIndex >= 0 && currentIndex < options.Length
            ? OptionResolver.GetDisplayName(options[currentIndex])
            : string.Empty);
    }

    private void UpdateDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            if (dots[i] is null)
                continue;

            dots[i].color = i == currentIndex ? carouselStyle.SelectedDotColor : carouselStyle.UnselectedDotColor;
        }
    }

    protected override void OnUnbind()
    {
        if (onPreviousClicked is not null && previousButton is not null)
            previousButton.onClick.RemoveListener(onPreviousClicked);

        if (onNextClicked is not null && nextButton is not null)
            nextButton.onClick.RemoveListener(onNextClicked);

        onPreviousClicked = null;
        onNextClicked = null;
        dots.Clear();
    }
}
