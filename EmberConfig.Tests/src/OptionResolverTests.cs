namespace EmberConfig.Tests;

using System;
using System.IO;
using BepInEx.Configuration;
using EmberConfig.Core;
using EmberConfig.UI;
using Xunit;

public class OptionResolverTests
{
    private static ConfigFile NewConfig() => new(Path.Combine(Path.GetTempPath(), $"EmberConfigTests-{Guid.NewGuid()}.cfg"), true);

    [Fact]
    public void Resolve_AcceptableValueList_ReturnsListValues()
    {
        var entry = CreateEntry("key", "b", new AcceptableValueList<string>("a", "b", "c"));

        var result = OptionResolver.Resolve(entry);

        Assert.Equal(3, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public void Resolve_Enum_ReturnsEnumValues()
    {
        var entry = CreateEntry("key", TestEnum.B);

        var result = OptionResolver.Resolve(entry);

        Assert.Equal(2, result.Length);
        Assert.Equal(TestEnum.A, result[0]);
        Assert.Equal(TestEnum.B, result[1]);
    }

    [Fact]
    public void Resolve_NoListOrEnum_ReturnsCurrentValue()
    {
        var entry = CreateEntry("key", 42);

        var result = OptionResolver.Resolve(entry);

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public void GetDisplayName_NullValue_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, OptionResolver.GetDisplayName(null));
    }

    [Fact]
    public void FindIndexByDisplayName_MatchesByDisplayName()
    {
        var options = new object?[] { TestEnum.A, TestEnum.B };

        var index = OptionResolver.FindIndexByDisplayName(options, TestEnum.B);

        Assert.Equal(1, index);
    }

    [Fact]
    public void FindIndexByDisplayName_NoMatch_ReturnsMinusOne()
    {
        var options = new object?[] { 1, 2, 3 };

        var index = OptionResolver.FindIndexByDisplayName(options, 4);

        Assert.Equal(-1, index);
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
