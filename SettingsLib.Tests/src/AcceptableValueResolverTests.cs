namespace SettingsLib.Tests;

using BepInEx.Configuration;
using SettingsLib.UI;
using Xunit;

public class AcceptableValueResolverTests
{
    [Fact]
    public void TryGetList_StringList_ReturnsValues()
    {
        var acceptable = new AcceptableValueList<string>("a", "b", "c");

        var result = AcceptableValueResolver.TryGetList(acceptable);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public void TryGetList_IntList_ReturnsValues()
    {
        var acceptable = new AcceptableValueList<int>(0, 1, 2);

        var result = AcceptableValueResolver.TryGetList(acceptable);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Length);
        Assert.Equal(0, result[0]);
        Assert.Equal(1, result[1]);
        Assert.Equal(2, result[2]);
    }

    [Fact]
    public void TryGetList_NullAcceptable_ReturnsNull()
    {
        var result = AcceptableValueResolver.TryGetList(null);

        Assert.Null(result);
    }

    [Fact]
    public void TryGetList_RangeAcceptable_ReturnsNull()
    {
        var acceptable = new AcceptableValueRange<int>(0, 10);

        var result = AcceptableValueResolver.TryGetList(acceptable);

        Assert.Null(result);
    }

    [Fact]
    public void TryGetRange_IntRange_ReturnsMinMax()
    {
        var acceptable = new AcceptableValueRange<int>(0, 100);

        var success = AcceptableValueResolver.TryGetRange(acceptable, out var min, out var max);

        Assert.True(success);
        Assert.Equal(0f, min);
        Assert.Equal(100f, max);
    }

    [Fact]
    public void TryGetRange_FloatRange_ReturnsMinMax()
    {
        var acceptable = new AcceptableValueRange<float>(0f, 1f);

        var success = AcceptableValueResolver.TryGetRange(acceptable, out var min, out var max);

        Assert.True(success);
        Assert.Equal(0f, min);
        Assert.Equal(1f, max);
    }

    [Fact]
    public void TryGetRange_ListAcceptable_ReturnsFalse()
    {
        var acceptable = new AcceptableValueList<string>("a", "b");

        var success = AcceptableValueResolver.TryGetRange(acceptable, out var min, out var max);

        Assert.False(success);
        Assert.Equal(0f, min);
        Assert.Equal(0f, max);
    }

    [Fact]
    public void TryGetRange_NullAcceptable_ReturnsFalse()
    {
        var success = AcceptableValueResolver.TryGetRange(null, out var min, out var max);

        Assert.False(success);
        Assert.Equal(0f, min);
        Assert.Equal(0f, max);
    }
}
