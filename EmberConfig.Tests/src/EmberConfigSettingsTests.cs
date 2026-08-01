namespace EmberConfig.Tests;

using Xunit;

public class EmberConfigSettingsTests
{
    public EmberConfigSettingsTests()
    {
        EmberConfigSettings.TabScrollSensitivity = EmberConfigSettings.DefaultTabScrollSensitivity;
        EmberConfigSettings.TabWidthScaling = EmberConfigSettings.DefaultTabWidthScaling;
        EmberConfigSettings.TabScrollAnimationDuration = EmberConfigSettings.DefaultTabScrollAnimationDuration;
    }

    [Fact]
    public void TabScrollSensitivity_DefaultsToDefaultValue()
    {
        Assert.Equal(EmberConfigSettings.DefaultTabScrollSensitivity, EmberConfigSettings.TabScrollSensitivity);
    }

    [Theory]
    [InlineData(10f, 20f)]
    [InlineData(20f, 20f)]
    [InlineData(80f, 80f)]
    [InlineData(200f, 200f)]
    [InlineData(250f, 200f)]
    public void TabScrollSensitivity_ClampsToRange(float value, float expected)
    {
        EmberConfigSettings.TabScrollSensitivity = value;
        Assert.Equal(expected, EmberConfigSettings.TabScrollSensitivity, precision: 5);
    }

    [Fact]
    public void TabScrollSensitivity_RaisesEventOnChange()
    {
        float? captured = null;
        EmberConfigSettings.TabScrollSensitivityChanged += v => captured = v;

        EmberConfigSettings.TabScrollSensitivity = 100f;

        Assert.NotNull(captured);
        Assert.Equal(100f, captured.Value, precision: 5);
    }

    [Fact]
    public void TabScrollSensitivity_DoesNotRaiseEventWhenUnchanged()
    {
        int callCount = 0;
        EmberConfigSettings.TabScrollSensitivityChanged += _ => callCount++;

        EmberConfigSettings.TabScrollSensitivity = EmberConfigSettings.TabScrollSensitivity;

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void TabWidthScaling_DefaultsToDefaultValue()
    {
        Assert.Equal(EmberConfigSettings.DefaultTabWidthScaling, EmberConfigSettings.TabWidthScaling);
    }

    [Theory]
    [InlineData(0f, 25f)]
    [InlineData(25f, 25f)]
    [InlineData(100f, 100f)]
    [InlineData(200f, 200f)]
    [InlineData(250f, 200f)]
    public void TabWidthScaling_ClampsToRange(float value, float expected)
    {
        EmberConfigSettings.TabWidthScaling = value;
        Assert.Equal(expected, EmberConfigSettings.TabWidthScaling, precision: 5);
    }

    [Fact]
    public void TabWidthScaling_RaisesEventOnChange()
    {
        float? captured = null;
        EmberConfigSettings.TabWidthScalingChanged += v => captured = v;

        EmberConfigSettings.TabWidthScaling = 150f;

        Assert.NotNull(captured);
        Assert.Equal(150f, captured.Value, precision: 5);
    }

    [Fact]
    public void TabWidthScaling_DoesNotRaiseEventWhenUnchanged()
    {
        int callCount = 0;
        EmberConfigSettings.TabWidthScalingChanged += _ => callCount++;

        EmberConfigSettings.TabWidthScaling = EmberConfigSettings.TabWidthScaling;

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void TabScrollAnimationDuration_DefaultsToDefaultValue()
    {
        Assert.Equal(EmberConfigSettings.DefaultTabScrollAnimationDuration, EmberConfigSettings.TabScrollAnimationDuration);
    }

    [Theory]
    [InlineData(0f, 0.05f)]
    [InlineData(0.05f, 0.05f)]
    [InlineData(0.25f, 0.25f)]
    [InlineData(0.80f, 0.80f)]
    [InlineData(1f, 0.80f)]
    public void TabScrollAnimationDuration_ClampsToRange(float value, float expected)
    {
        EmberConfigSettings.TabScrollAnimationDuration = value;
        Assert.Equal(expected, EmberConfigSettings.TabScrollAnimationDuration, precision: 5);
    }

    [Fact]
    public void TabScrollAnimationDuration_RaisesEventOnChange()
    {
        float? captured = null;
        EmberConfigSettings.TabScrollAnimationDurationChanged += v => captured = v;

        EmberConfigSettings.TabScrollAnimationDuration = 0.25f;

        Assert.NotNull(captured);
        Assert.Equal(0.25f, captured.Value, precision: 5);
    }

    [Fact]
    public void TabScrollAnimationDuration_DoesNotRaiseEventWhenUnchanged()
    {
        int callCount = 0;
        EmberConfigSettings.TabScrollAnimationDurationChanged += _ => callCount++;

        EmberConfigSettings.TabScrollAnimationDuration = EmberConfigSettings.TabScrollAnimationDuration;

        Assert.Equal(0, callCount);
    }
}
