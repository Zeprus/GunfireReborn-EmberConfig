namespace EmberConfig.Tests;

using System;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using EmberConfig.Core;
using Xunit;

public class SettingsRegistryTests
{
    private static ConfigFile NewConfig() => new(Path.Combine(Path.GetTempPath(), $"EmberConfigTests-{Guid.NewGuid()}.cfg"), true);

    [Fact]
    public void Register_AddsEntry()
    {
        var registry = new SettingsRegistry();
        var entry = CreateEntry("test", "MyTab", "General", null);

        registry.Register(entry);

        Assert.Single(registry.Entries);
    }

    [Fact]
    public void GetByTab_IsCaseInsensitive()
    {
        var registry = new SettingsRegistry();
        registry.Register(CreateEntry("a", "MyTab", "General", null));

        Assert.Single(registry.GetByTab("mytab"));
    }

    [Fact]
    public void GetByTab_ReturnsEmpty_WhenNotFound()
    {
        var registry = new SettingsRegistry();

        Assert.Empty(registry.GetByTab("Unknown"));
    }

    [Fact]
    public void GetByGroup_IsCaseInsensitiveAndTrimsWhitespace()
    {
        var registry = new SettingsRegistry();
        registry.Register(CreateEntry("a", "Tab", "  General  ", null));

        Assert.Single(registry.GetByGroup("tab", "general"));
    }

    [Fact]
    public void GetKeybindEntries_ReturnsOnlyKeybinds()
    {
        var registry = new SettingsRegistry();
        registry.Register(CreateEntry("a", "Tab", "G", null));

        Assert.Empty(registry.GetKeybindEntries());
    }

    [Fact]
    public void GetTabs_ReturnsRegisteredTabs()
    {
        var registry = new SettingsRegistry();
        registry.Register(CreateEntry("a", "TabA", "G", null));
        registry.Register(CreateEntry("b", "TabB", "G", null));

        var tabs = registry.GetTabs().ToList();

        Assert.Equal(2, tabs.Count);
        Assert.Contains("TabA", tabs, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("TabB", tabs, StringComparer.OrdinalIgnoreCase);
    }

    private static ISettingEntry CreateEntry(string id, string tab, string group, string? subGroup)
    {
        var config = NewConfig();
        var setting = config.Bind("test", id, "value");
        return new SettingEntry<string>(id, setting, $"Label {id}", new SettingLocation(tab, group, subGroup));
    }
}
