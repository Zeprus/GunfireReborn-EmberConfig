namespace SettingsLib.Tests;

using SettingsLib.UI;
using Xunit;

public class TabBarLayoutTests
{
    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(4, 5, 4)]
    [InlineData(5, 5, 0)]
    [InlineData(6, 5, 1)]
    [InlineData(-1, 5, 4)]
    [InlineData(-5, 5, 0)]
    [InlineData(-6, 5, 4)]
    public void Mod_WrapsCorrectly(int value, int length, int expected)
    {
        Assert.Equal(expected, TabBarLayout.Mod(value, length));
    }

    [Fact]
    public void Mod_ZeroLength_ReturnsZero()
    {
        Assert.Equal(0, TabBarLayout.Mod(10, 0));
    }

    [Theory]
    [InlineData(0, 2, 5, 2)]
    [InlineData(0, 3, 5, -2)]
    [InlineData(0, 4, 5, -1)]
    [InlineData(0, -1, 5, -1)]
    [InlineData(4, 0, 5, 1)]
    [InlineData(4, 1, 5, 2)]
    [InlineData(1, 4, 5, -2)]
    [InlineData(1, 0, 5, -1)]
    public void ShortestDelta_ChoosesShortestPath(int from, int to, int length, int expected)
    {
        Assert.Equal(expected, TabBarLayout.ShortestDelta(from, to, length));
    }

    [Fact]
    public void ShortestDelta_ZeroLength_ReturnsZero()
    {
        Assert.Equal(0, TabBarLayout.ShortestDelta(0, 1, 0));
    }

    [Fact]
    public void ComputeScrollOffset_ContentFits_ReturnsZero()
    {
        // Viewport 0..1000, content 0..800, active 0..100.
        var offset = TabBarLayout.ComputeScrollOffset(1000f, 800f, 0f);

        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ComputeScrollOffset_FirstTabLeftAnchored_ReturnsZero()
    {
        // Viewport 0..100, content 0..1000, active starts at 0.
        var offset = TabBarLayout.ComputeScrollOffset(100f, 1000f, 0f);

        // Active left edge should align with viewport left edge.
        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ComputeScrollOffset_LastTabClampsToRightEdge()
    {
        // Viewport 0..100, content 0..1000, active 900..1000.
        var offset = TabBarLayout.ComputeScrollOffset(100f, 1000f, 900f);

        // Scrollable distance is 900; active left edge at 900 means
        // content is shifted left by 900 so the right edge touches.
        Assert.Equal(-900f, offset);
    }

    [Fact]
    public void ComputeScrollOffset_MiddleTabLeftAnchored()
    {
        // Viewport 0..100, content 0..1000, active 450..550.
        var offset = TabBarLayout.ComputeScrollOffset(100f, 1000f, 450f);

        // Active left edge should align with viewport left edge.
        Assert.Equal(-450f, offset);
    }

    [Fact]
    public void ComputeScrollOffset_PastEndClamps()
    {
        // Viewport 0..100, content 0..1000, active 950..1050.
        var offset = TabBarLayout.ComputeScrollOffset(100f, 1000f, 950f);

        // Cannot scroll past the last tab's right edge.
        Assert.Equal(-900f, offset);
    }

    [Fact]
    public void ComputeHorizontalNormalizedPosition_FirstTabIsZero()
    {
        var normalized = TabBarLayout.ComputeHorizontalNormalizedPosition(100f, 1000f, 0f);

        Assert.Equal(0f, normalized);
    }

    [Fact]
    public void ComputeHorizontalNormalizedPosition_LastTabIsOne()
    {
        var normalized = TabBarLayout.ComputeHorizontalNormalizedPosition(100f, 1000f, 900f);

        Assert.Equal(1f, normalized);
    }

    [Fact]
    public void ComputeHorizontalNormalizedPosition_MiddleTab()
    {
        var normalized = TabBarLayout.ComputeHorizontalNormalizedPosition(100f, 1000f, 450f);

        Assert.Equal(0.5f, normalized, precision: 3);
    }

    [Fact]
    public void ComputeHorizontalNormalizedPosition_PastEndClampsToOne()
    {
        var normalized = TabBarLayout.ComputeHorizontalNormalizedPosition(100f, 1000f, 950f);

        Assert.Equal(1f, normalized);
    }

    [Fact]
    public void ComputeHorizontalNormalizedPosition_ContentFitsIsZero()
    {
        var normalized = TabBarLayout.ComputeHorizontalNormalizedPosition(1000f, 800f, 300f);

        Assert.Equal(0f, normalized);
    }
}
