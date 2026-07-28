namespace SettingsLib.Tests;

using System;
using System.IO;
using BepInEx.Configuration;
using SettingsLib.Core;
using SettingsLib.UI;
using UnityEngine;
using Xunit;

public class RowTypeResolverTests
{
    private static ConfigFile NewConfig() => new(Path.Combine(Path.GetTempPath(), $"SettingsLibTests-{Guid.NewGuid()}.cfg"), true);

    [Fact]
    public void Bool_ReturnsToggle()
    {
        var entry = CreateEntry("key", true);
        Assert.Equal(RowType.Toggle, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void IntWithRange_ReturnsSlider()
    {
        var entry = CreateEntry("key", 5, new AcceptableValueRange<int>(0, 10));
        Assert.Equal(RowType.Slider, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void IntWithoutRange_ReturnsInputField()
    {
        var entry = CreateEntry("key", 5);
        Assert.Equal(RowType.InputField, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void FloatWithoutRange_ReturnsInputField()
    {
        var entry = CreateEntry("key", 0.5f);
        Assert.Equal(RowType.InputField, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void FloatWithList_ReturnsDropdown()
    {
        var entry = CreateEntry("key", 0f, new AcceptableValueList<float>(0f, 0.5f, 1f));
        Assert.Equal(RowType.Dropdown, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void FloatWithRange_ReturnsSlider()
    {
        var entry = CreateEntry("key", 0.5f, new AcceptableValueRange<float>(0f, 1f));
        Assert.Equal(RowType.Slider, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void Enum_ReturnsDropdown()
    {
        var entry = CreateEntry("key", TestEnum.A);
        Assert.Equal(RowType.Dropdown, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void StringList_ReturnsDropdown()
    {
        var entry = CreateEntry("key", "a", new AcceptableValueList<string>("a", "b"));
        Assert.Equal(RowType.Dropdown, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void IntList_ReturnsDropdown()
    {
        var entry = CreateEntry("key", 0, new AcceptableValueList<int>(0, 1, 2));
        Assert.Equal(RowType.Dropdown, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void String_ReturnsInputField()
    {
        var entry = CreateEntry("key", "value");
        Assert.Equal(RowType.InputField, RowTypeResolver.Resolve(entry));
    }

    [Fact]
    public void KeybindEntry_ReturnsKeybind()
    {
        var config = NewConfig();
        var keybind = config.Bind("test", "key", KeyCode.A);
        var entry = new KeybindEntry("id", keybind, null, "label", new SettingLocation("Tab", "Group"), null, null);

        Assert.Equal(RowType.Keybind, RowTypeResolver.Resolve(entry));
    }

    private static ISettingEntry CreateEntry<T>(string key, T value, AcceptableValueBase? acceptable = null)
    {
        var config = NewConfig();
        var description = acceptable is not null ? new ConfigDescription("", acceptable) : null;
        var setting = config.Bind("test", key, value, description);
        return new SettingEntry<T>(key, setting, "Label", new SettingLocation("Tab", "Group"));
    }

    private enum TestEnum { A, B }
}
