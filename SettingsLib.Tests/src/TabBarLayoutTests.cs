namespace SettingsLib.Tests;

using SettingsLib.UI;
using Xunit;

public class TabBarLayoutTests
{
    [Fact]
    public void ActiveVisible_FitsViewport_ReturnsCurrentOffset()
    {
        // Viewport 0..1000, content 0..800, active 0..100, current offset 0.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 1000f,
            0f, 800f,
            0f, 100f,
            0f);

        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ActiveVisibleButOffsetOutOfRange_KeepsCurrentOffset()
    {
        // Content fits, but current offset is too far right; active tab is still visible.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 1000f,
            0f, 800f,
            0f, 100f,
            500f);

        // Because the active tab is already fully visible, the current offset is preserved.
        Assert.Equal(500f, offset);
    }

    [Fact]
    public void ActiveOffRight_ScrollsLeftToBringItIntoView()
    {
        // Viewport 0..100, content 0..1000, active 900..1000, current offset 0.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 100f,
            0f, 1000f,
            900f, 1000f,
            0f);

        // Target centers active at viewport center 50: 50 - 950 = -900.
        // Allowed range is [-900, 0]; target equals lower bound.
        Assert.Equal(-900f, offset);
    }

    [Fact]
    public void ActiveOffLeft_ScrollsRightToBringItIntoView()
    {
        // Viewport 0..100, content 0..1000, active 0..100, current offset -500.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 100f,
            0f, 1000f,
            0f, 100f,
            -500f);

        // Active is off-screen to the left. Target centers it at 50: 50 - 50 = 0.
        // Allowed range is [-900, 0]; target clamps to upper bound 0.
        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ActiveInMiddleWithOverflow_CentersActive()
    {
        // Viewport 0..100, content 0..1000, active 450..550, current offset 0.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 100f,
            0f, 1000f,
            450f, 550f,
            0f);

        // Target centers active at viewport center 50: 50 - 500 = -450.
        // Allowed range [-900, 0] includes -450.
        Assert.Equal(-450f, offset);
    }

    [Fact]
    public void ActiveVisible_DoesNotRecenterByDefault()
    {
        // Viewport 0..1000, content 0..800, active 0..100, current offset 0.
        // Active is visible on the left side and not centered.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 1000f,
            0f, 800f,
            0f, 100f,
            0f);

        // Because the active tab is already fully visible and recenter is false,
        // the current offset should be preserved (clamped to the allowed range).
        Assert.Equal(0f, offset);
    }

    [Fact]
    public void RecenterIfVisible_CentersActiveEvenWhenVisible()
    {
        // Viewport 0..1000, content 0..1000, active 400..600, current offset 0.
        // Active is visible and centered; recenter should return 0.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 1000f,
            0f, 1000f,
            400f, 600f,
            0f,
            recenterIfVisible: true);

        // Center active at viewport center 500: 500 - 500 = 0.
        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ClampedByFirstTab_CannotScrollPastLeftEdge()
    {
        // Viewport 0..100, content 0..1000, active 0..100, current offset -500.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 100f,
            0f, 1000f,
            0f, 100f,
            -500f);

        // Target = 50 - 50 = 0; clamped to upper bound 0.
        Assert.Equal(0f, offset);
    }

    [Fact]
    public void ClampedByLastTab_CannotScrollPastRightEdge()
    {
        // Viewport 0..100, content 0..1000, active 900..1000, current offset -1000.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 100f,
            0f, 1000f,
            900f, 1000f,
            -1000f);

        // Target = 50 - 950 = -900; clamped to lower bound -900.
        Assert.Equal(-900f, offset);
    }

    [Fact]
    public void ContentFits_ActiveCenteredInViewport()
    {
        // Viewport 0..1000, content 0..800, active 300..500.
        var offset = TabBarLayout.ComputeScrollOffset(
            0f, 1000f,
            0f, 800f,
            300f, 500f,
            0f,
            recenterIfVisible: true);

        // Center active at viewport center 500: 500 - 400 = 100.
        // Allowed range [0, 200] includes 100.
        Assert.Equal(100f, offset);
    }
}
