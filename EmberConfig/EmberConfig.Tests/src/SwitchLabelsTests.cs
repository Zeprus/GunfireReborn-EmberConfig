namespace EmberConfig.Tests;

using System;
using System.IO;
using BepInEx.Configuration;
using EmberConfig.Core;
using EmberConfig.Public;
using EmberConfig.UI;
using Xunit;

public class SwitchLabelsTests
{
    private static ConfigFile NewConfig() => new(Path.Combine(Path.GetTempPath(), $"EmberConfigTests-{Guid.NewGuid()}.cfg"), true);

    [Fact]
    public void SettingEntry_PreservesSwitchLabels()
    {
        var config = NewConfig();
        var setting = config.Bind("test", "key", true);
        var labels = new SwitchLabels("Show", "Hide");

        var entry = new SettingEntry<bool>("id", setting, "Label", new SettingLocation("Tab", "Group"), null, SettingControlStyle.Switch, labels);

        Assert.Equal(labels, entry.SwitchLabels);
    }

    [Fact]
    public void SettingEntry_SwitchLabels_NullByDefault()
    {
        var config = NewConfig();
        var setting = config.Bind("test", "key", true);

        var entry = new SettingEntry<bool>("id", setting, "Label", new SettingLocation("Tab", "Group"));

        Assert.Null(entry.SwitchLabels);
    }

    [Fact]
    public void SwitchStyle_CanOverrideLabels()
    {
        var style = new SwitchStyle(
            default,
            default,
            null,
            null,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default)
        {
            Option1Label = "On",
            Option2Label = "Off",
        };

        var overridden = style with { Option1Label = "Show", Option2Label = "Hide" };

        Assert.Equal("Show", overridden.Option1Label);
        Assert.Equal("Hide", overridden.Option2Label);
    }
}
