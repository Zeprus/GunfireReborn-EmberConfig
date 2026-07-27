namespace SettingsLib.Tests;

using SettingsLib.Core;
using Xunit;

public class SettingLocationTests
{
    [Fact]
    public void Equality_IsValueBased()
    {
        var a = new SettingLocation("Tab", "Group", "Sub");
        var b = new SettingLocation("Tab", "Group", "Sub");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void SubGroup_CanBeNull()
    {
        var a = new SettingLocation("Tab", "Group");
        var b = new SettingLocation("Tab", "Group", null);

        Assert.Equal(a, b);
    }

    [Fact]
    public void SubGroup_DistinguishesNullAndEmpty()
    {
        var a = new SettingLocation("Tab", "Group");
        var b = new SettingLocation("Tab", "Group", string.Empty);

        Assert.NotEqual(a, b);
    }
}
