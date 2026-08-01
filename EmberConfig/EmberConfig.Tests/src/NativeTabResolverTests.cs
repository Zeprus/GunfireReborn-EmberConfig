namespace EmberConfig.Tests;

using System.Linq;
using EmberConfig.UI;
using Xunit;

public class NativeTabResolverTests
{
    [Theory]
    [InlineData("Game Settings", "Content_1")]
    [InlineData("Mouse/Keyboard", "Content_2")]
    [InlineData("Video", "Content_3")]
    [InlineData("Audio", "Content_4")]
    [InlineData("Controller", "Content_5")]
    public void TryGetNativeContentName_ReturnsExpectedMapping(string tabName, string expectedContent)
    {
        var found = NativeTabResolver.TryGetNativeContentName(tabName, out var contentName);

        Assert.True(found);
        Assert.Equal(expectedContent, contentName);
    }

    [Theory]
    [InlineData("game settings")]
    [InlineData("GAME SETTINGS")]
    [InlineData("  Game Settings  ")]
    public void TryGetNativeContentName_IsCaseInsensitiveAndTrims(string tabName)
    {
        var found = NativeTabResolver.TryGetNativeContentName(tabName, out var contentName);

        Assert.True(found);
        Assert.Equal("Content_1", contentName);
    }

    [Fact]
    public void TryGetNativeContentName_ReturnsFalseForUnknownTab()
    {
        var found = NativeTabResolver.TryGetNativeContentName("Mods", out var contentName);

        Assert.False(found);
        Assert.Null(contentName);
    }

    [Fact]
    public void IsNativeTab_RecognizesNativeTabs()
    {
        Assert.True(NativeTabResolver.IsNativeTab("Video"));
        Assert.False(NativeTabResolver.IsNativeTab("My Custom Tab"));
    }

    [Fact]
    public void GetNativeContentNames_ReturnsDistinctContentNames()
    {
        var names = NativeTabResolver.GetNativeContentNames().ToList();

        Assert.Equal(5, names.Count);
        Assert.Contains("Content_1", names);
        Assert.Contains("Content_5", names);
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Theory]
    [InlineData("Content_1", "Game Settings")]
    [InlineData("Content_2", "Mouse/Keyboard")]
    [InlineData("Content_3", "Video")]
    [InlineData("Content_4", "Audio")]
    [InlineData("Content_5", "Controller")]
    public void TryGetNativeTabName_ReturnsExpectedMapping(string contentName, string expectedTab)
    {
        var found = NativeTabResolver.TryGetNativeTabName(contentName, out var tabName);

        Assert.True(found);
        Assert.Equal(expectedTab, tabName);
    }

    [Fact]
    public void TryGetNativeTabName_ReturnsFalseForUnknownContent()
    {
        var found = NativeTabResolver.TryGetNativeTabName("Content_99", out var tabName);

        Assert.False(found);
        Assert.Null(tabName);
    }

    [Theory]
    [InlineData("Game Settings", 0)]
    [InlineData("Mouse/Keyboard", 1)]
    [InlineData("Video", 2)]
    [InlineData("Audio", 3)]
    [InlineData("Controller", 4)]
    public void GetNativeTabOrder_ReturnsExpectedIndex(string tabName, int expectedOrder)
    {
        Assert.Equal(expectedOrder, NativeTabResolver.GetNativeTabOrder(tabName));
    }

    [Fact]
    public void GetNativeTabOrder_ReturnsMaxForUnknownTab()
    {
        Assert.Equal(int.MaxValue, NativeTabResolver.GetNativeTabOrder("Unknown"));
    }
}
