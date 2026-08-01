namespace EmberConfig.Tests;

using System;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using EmberConfig.Core;
using Xunit;

public class VisibilityStoreTests
{
    private static ConfigFile NewConfig() => new(Path.Combine(Path.GetTempPath(), $"EmberConfigTests-{Guid.NewGuid()}.cfg"), true);

    public VisibilityStoreTests()
    {
        SettingsRegistry.Current = new SettingsRegistry();
    }

    [Fact]
    public void IsVisible_ReturnsTrue_ByDefault()
    {
        var store = new VisibilityStore(NewConfig());

        Assert.True(store.IsVisible("MyMod", "Game Settings"));
    }

    [Fact]
    public void GetOrCreate_ReturnsSameEntry_ForSameModAndTab()
    {
        var store = new VisibilityStore(NewConfig());

        var first = store.GetOrCreate("MyMod", "Game Settings");
        var second = store.GetOrCreate("MyMod", "Game Settings");

        Assert.Same(first, second);
    }

    [Fact]
    public void IsVisible_ReturnsFalse_WhenConfigIsFalse()
    {
        var store = new VisibilityStore(NewConfig());

        var entry = store.GetOrCreate("MyMod", "Game Settings");
        entry.Value = false;

        Assert.False(store.IsVisible("MyMod", "Game Settings"));
    }

    [Fact]
    public void IsVisible_ReturnsTrue_ForSentinel()
    {
        var store = new VisibilityStore(NewConfig());

        Assert.True(store.IsVisible(VisibilityStore.SentinelModName, VisibilityStore.VisibilityTab));
    }

    [Fact]
    public void IsVisible_ReturnsTrue_ForEmberConfigTab()
    {
        var store = new VisibilityStore(NewConfig());

        Assert.True(store.IsVisible("EmberConfig", VisibilityStore.VisibilityTab));
    }

    [Fact]
    public void EnsureVisibilitySwitch_SkipsEmberConfigTab()
    {
        var store = new VisibilityStore(NewConfig());
        var entry = CreateConsumer("EmberConfig", VisibilityStore.VisibilityTab, "Group");

        store.EnsureVisibilitySwitch(entry);

        Assert.Empty(SettingsRegistry.Current.Entries.OfType<ISettingEntry>().Where(e => e.ModName == VisibilityStore.SentinelModName));
    }

    [Fact]
    public void EnsureVisibilitySwitch_CreatesVisibilityRow()
    {
        var store = new VisibilityStore(NewConfig());
        var entry = CreateConsumer("MyMod", "Game Settings", "Group");

        store.EnsureVisibilitySwitch(entry);

        var visibilityRow = SettingsRegistry.Current.Entries
            .OfType<SettingEntry<bool>>()
            .FirstOrDefault(e => e.ModName == VisibilityStore.SentinelModName);

        Assert.NotNull(visibilityRow);
        Assert.Equal(VisibilityStore.VisibilityTab, visibilityRow.Location.Tab);
        Assert.Equal(VisibilityStore.VisibilityGroup, visibilityRow.Location.Group);
        Assert.Equal("MyMod", visibilityRow.Location.SubGroup);
        Assert.Equal("Game Settings", visibilityRow.Label);
    }

    [Fact]
    public void EnsureVisibilitySwitch_SkipsDuplicate()
    {
        var store = new VisibilityStore(NewConfig());
        var first = CreateConsumer("MyMod", "Game Settings", "Group");
        var second = CreateConsumer("MyMod", "Game Settings", "Other");

        store.EnsureVisibilitySwitch(first);
        store.EnsureVisibilitySwitch(second);

        var visibilityRows = SettingsRegistry.Current.Entries
            .OfType<SettingEntry<bool>>()
            .Where(e => e.ModName == VisibilityStore.SentinelModName)
            .ToList();

        Assert.Single(visibilityRows);
    }

    [Fact]
    public void EnsureVisibilitySwitch_CreatesMultipleRows_ForDifferentTabs()
    {
        var store = new VisibilityStore(NewConfig());
        var game = CreateConsumer("MyMod", "Game Settings", "Group");
        var audio = CreateConsumer("MyMod", "Audio", "Group");

        store.EnsureVisibilitySwitch(game);
        store.EnsureVisibilitySwitch(audio);

        var visibilityRows = SettingsRegistry.Current.Entries
            .OfType<SettingEntry<bool>>()
            .Where(e => e.ModName == VisibilityStore.SentinelModName)
            .ToList();

        Assert.Equal(2, visibilityRows.Count);
    }

    [Fact]
    public void ResetAllVisibility_RemovesAndRecreatesSwitchesWithDefaultValue()
    {
        var config = NewConfig();
        var store = new VisibilityStore(config);
        var entry = CreateConsumer("MyMod", "Game Settings", "Group");

        store.EnsureVisibilitySwitch(entry);
        var first = store.GetOrCreate("MyMod", "Game Settings");
        first.Value = false;

        store.ResetAllVisibility();

        var visibilityRows = SettingsRegistry.Current.Entries
            .OfType<SettingEntry<bool>>()
            .Where(e => e.ModName == VisibilityStore.SentinelModName)
            .ToList();

        Assert.Single(visibilityRows);
        Assert.True(visibilityRows[0].Config.Value);
    }

    private static ISettingEntry CreateConsumer(string modName, string tab, string? group)
    {
        var config = NewConfig();
        var setting = config.Bind("test", "key", "value");
        var entry = new SettingEntry<string>(Guid.NewGuid().ToString("N"), setting, "Label", modName, new SettingLocation(tab, group));
        SettingsRegistry.Current.Register(entry);
        return entry;
    }
}
