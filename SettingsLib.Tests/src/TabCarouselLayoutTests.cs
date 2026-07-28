namespace SettingsLib.Tests;

using SettingsLib.UI;
using Xunit;

public class TabCarouselLayoutTests
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
        Assert.Equal(expected, TabCarouselLayout.Mod(value, length));
    }

    [Fact]
    public void Mod_ZeroLength_ReturnsZero()
    {
        Assert.Equal(0, TabCarouselLayout.Mod(10, 0));
    }

    [Theory]
    [InlineData(0f, 2, 5, 2f)]
    [InlineData(0f, 3, 5, -2f)] // shorter to wrap backwards
    [InlineData(0f, 4, 5, -1f)]
    [InlineData(0f, -1, 5, -1f)] // -1 modulo 5 == 4
    [InlineData(4f, 0, 5, 1f)]
    [InlineData(4f, 1, 5, 2f)]
    [InlineData(1f, 4, 5, -2f)]
    [InlineData(1f, 0, 5, -1f)]
    public void ShortestDelta_ChoosesShortestPath(float from, int to, int length, float expected)
    {
        Assert.Equal(expected, TabCarouselLayout.ShortestDelta(from, to, length), precision: 3);
    }

    [Fact]
    public void ShortestDelta_ZeroLength_ReturnsZero()
    {
        Assert.Equal(0f, TabCarouselLayout.ShortestDelta(0f, 1, 0));
    }

    [Theory]
    [InlineData(5f, -2, 3)]
    [InlineData(5f, -1, 4)]
    [InlineData(5f, 0, 5)]
    [InlineData(5f, 1, 6)]
    [InlineData(5f, 2, 7)]
    [InlineData(5.4f, 2, 7)] // rounding
    [InlineData(5.6f, 2, 8)]
    public void GetDesiredContentIndex_ReturnsRoundedOffset(float currentActive, int offset, int expected)
    {
        Assert.Equal(expected, TabCarouselLayout.GetDesiredContentIndex(currentActive, offset));
    }

    [Theory]
    [InlineData(5f, 8)] // ceil(5 + 3)
    [InlineData(5.4f, 9)] // ceil(8.4)
    [InlineData(5.6f, 9)] // ceil(8.6)
    public void GetRecycledRightContentIndex_RoundsUp(float currentActive, int expected)
    {
        Assert.Equal(expected, TabCarouselLayout.GetRecycledRightContentIndex(currentActive));
    }

    [Theory]
    [InlineData(5f, 2)] // floor(5 - 3)
    [InlineData(5.4f, 2)] // floor(2.4)
    [InlineData(5.6f, 2)] // floor(2.6)
    public void GetRecycledLeftContentIndex_RoundsDown(float currentActive, int expected)
    {
        Assert.Equal(expected, TabCarouselLayout.GetRecycledLeftContentIndex(currentActive));
    }

    [Theory]
    [InlineData(5, 5f, 100f, 0f)]
    [InlineData(5, 6f, 100f, -100f)]
    [InlineData(5, 4f, 100f, 100f)]
    [InlineData(5, 7f, 100f, -200f)]
    public void GetVisualPosition_ComputesOffsetFromActive(int contentIndex, float currentActive, float step, float expected)
    {
        Assert.Equal(expected, TabCarouselLayout.GetVisualPosition(contentIndex, currentActive, step), precision: 3);
    }

    [Theory]
    [InlineData(-3.51f, true)]
    [InlineData(3.51f, true)]
    [InlineData(-3.5f, false)]
    [InlineData(3.5f, false)]
    [InlineData(0f, false)]
    public void IsOffScreen_DetectsRecycleThreshold(float visualPosition, bool expected)
    {
        Assert.Equal(expected, TabCarouselLayout.IsOffScreen(visualPosition, 1f));
    }

    [Theory]
    [InlineData(0f, 1f, 1f)]
    [InlineData(1f, 1f, 0.95f)]
    [InlineData(2f, 1f, 0.9f)]
    [InlineData(2.5f, 1f, 0.875f)]
    [InlineData(3f, 0f, 0.85f)]
    [InlineData(3.5f, 0f, 0.85f)]
    public void GetVisualState_FadesAndScalesWithDistance(float distance, float expectedAlpha, float expectedScale)
    {
        var (alpha, scale) = TabCarouselLayout.GetVisualState(distance);
        Assert.Equal(expectedAlpha, alpha, precision: 3);
        Assert.Equal(expectedScale, scale, precision: 3);
    }
}
