namespace EmberConfig.PrefabDataGen.Tests;

using System.IO;
using EmberConfig.PrefabDataGen.Parsing;
using Xunit;
using YamlDotNet.RepresentationModel;

public class YamlParsersTests
{
    [Fact]
    public void ParseColor_Rgba_ReturnsExpectedColor()
    {
        var mapping = ParseMapping("rgba: 4278190080");

        var result = YamlParsers.ParseColor(mapping);

        Assert.True(result.HasValue);
        Assert.Equal(1.0, (double)result.Value.R, 4);
        Assert.Equal(0.0, (double)result.Value.G, 4);
        Assert.Equal(0.0, (double)result.Value.B, 4);
        Assert.Equal(0.0, (double)result.Value.A, 4);
    }

    [Fact]
    public void ParseColor_Rgba_White_ReturnsAllOnes()
    {
        var mapping = ParseMapping("rgba: 4294967295");

        var result = YamlParsers.ParseColor(mapping);

        Assert.True(result.HasValue);
        Assert.Equal(1.0, (double)result.Value.R, 4);
        Assert.Equal(1.0, (double)result.Value.G, 4);
        Assert.Equal(1.0, (double)result.Value.B, 4);
        Assert.Equal(1.0, (double)result.Value.A, 4);
    }

    [Fact]
    public void ParseColor_SeparateChannels_ReturnsExpectedColor()
    {
        var mapping = ParseMapping("r: 0.1\ng: 0.2\nb: 0.3\na: 0.4");

        var result = YamlParsers.ParseColor(mapping);

        Assert.True(result.HasValue);
        Assert.Equal(0.1, (double)result.Value.R, 4);
        Assert.Equal(0.2, (double)result.Value.G, 4);
        Assert.Equal(0.3, (double)result.Value.B, 4);
        Assert.Equal(0.4, (double)result.Value.A, 4);
    }

    [Fact]
    public void ParseColor_InvalidMapping_ReturnsNull()
    {
        var mapping = ParseMapping("foo: bar");

        var result = YamlParsers.ParseColor(mapping);

        Assert.False(result.HasValue);
    }

    [Fact]
    public void ParseColor_NullMapping_ReturnsNull()
    {
        var result = YamlParsers.ParseColor(null);

        Assert.False(result.HasValue);
    }

    [Fact]
    public void ParseVector2_ReturnsExpectedVector()
    {
        var mapping = ParseMapping("x: 1.5\ny: -2.5");

        var result = YamlParsers.ParseVector2(mapping);

        Assert.True(result.HasValue);
        Assert.Equal(1.5, (double)result.Value.X, 4);
        Assert.Equal(-2.5, (double)result.Value.Y, 4);
    }

    [Fact]
    public void ParseVector2_InvalidMapping_ReturnsNull()
    {
        var mapping = ParseMapping("x: 1");

        var result = YamlParsers.ParseVector2(mapping);

        Assert.False(result.HasValue);
    }

    [Fact]
    public void ParseVector3_ReturnsExpectedVector()
    {
        var mapping = ParseMapping("x: 1\ny: 2\nz: 3");

        var result = YamlParsers.ParseVector3(mapping);

        Assert.True(result.HasValue);
        Assert.Equal(1.0, (double)result.Value.X, 4);
        Assert.Equal(2.0, (double)result.Value.Y, 4);
        Assert.Equal(3.0, (double)result.Value.Z, 4);
    }

    [Fact]
    public void ParseVector3_InvalidMapping_ReturnsNull()
    {
        var mapping = ParseMapping("x: 1\nz: 3");

        var result = YamlParsers.ParseVector3(mapping);

        Assert.False(result.HasValue);
    }

    private static YamlMappingNode ParseMapping(string yaml)
    {
        using var reader = new StringReader(yaml);
        var stream = new YamlStream();
        stream.Load(reader);
        return (YamlMappingNode)stream.Documents[0].RootNode;
    }
}
