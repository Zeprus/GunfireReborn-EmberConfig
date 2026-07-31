namespace EmberConfig.Tests;

using Xunit;

public class EmberConfigSettingsTests
{
    public EmberConfigSettingsTests()
    {
        EmberConfigSettings.TabScrollSensitivity = EmberConfigSettings.DefaultTabScrollSensitivity;
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
}
