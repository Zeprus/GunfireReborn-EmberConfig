namespace EmberConfig.Tests;

using EmberConfig.UI;
using Xunit;

public class TabBarScrollEasingTests
{
    [Theory]
    [InlineData(0f, 0f, 0.05f)]
    [InlineData(0.25f, 0f, 0.25f)]
    [InlineData(0.25f, 1f, 0.50f)]
    [InlineData(0.05f, 0.5f, 0.10f)]
    [InlineData(0.80f, 0f, 0.80f)]
    [InlineData(0.80f, 1f, 1.60f)]
    [InlineData(0.10f, 2f, 0.20f)]
    public void ComputeDuration_ReturnsClampedBaseTimesDistance(float baseDuration, float distance, float expected)
    {
        Assert.Equal(expected, TabBarScrollEasing.ComputeDuration(baseDuration, distance), precision: 5);
    }

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.5f, 0.875f)]
    public void EaseOutCubic_ProducesExpectedValues(float t, float expected)
    {
        Assert.Equal(expected, TabBarScrollEasing.EaseOutCubic(t), precision: 5);
    }

    [Theory]
    [InlineData(0f, 10f, 0f, 10f)]
    [InlineData(10f, 20f, 0f, 10f)]
    [InlineData(10f, 20f, 1f, 20f)]
    [InlineData(10f, 20f, 0.5f, 15f)]
    public void Lerp_InterpolatesLinearly(float a, float b, float t, float expected)
    {
        Assert.Equal(expected, TabBarScrollEasing.Lerp(a, b, t), precision: 5);
    }
}
