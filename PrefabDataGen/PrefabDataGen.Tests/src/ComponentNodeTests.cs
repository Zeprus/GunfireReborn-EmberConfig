namespace EmberConfig.PrefabDataGen.Tests;

using System.IO;
using EmberConfig.PrefabDataGen.Parsing;
using Xunit;
using YamlDotNet.RepresentationModel;

public class ComponentNodeTests
{
    [Fact]
    public void GetString_ReturnsValue()
    {
        var node = CreateComponent("m_Name: TestObject");

        Assert.Equal("TestObject", node.GetString("m_Name"));
    }

    [Fact]
    public void GetString_MissingKey_ReturnsNull()
    {
        var node = CreateComponent("m_Name: TestObject");

        Assert.Null(node.GetString("Missing"));
    }

    [Fact]
    public void GetInt_ReturnsValue()
    {
        var node = CreateComponent("m_IsActive: 1");

        Assert.Equal(1, node.GetInt("m_IsActive"));
    }

    [Fact]
    public void GetInt_MissingOrInvalid_ReturnsNull()
    {
        var node = CreateComponent("m_IsActive: notANumber");

        Assert.Null(node.GetInt("m_IsActive"));
        Assert.Null(node.GetInt("Missing"));
    }

    [Fact]
    public void GetFloat_ReturnsValue()
    {
        var node = CreateComponent("m_Value: 3.14");

        Assert.Equal(3.14, (double)node.GetFloat("m_Value")!, 4);
    }

    [Fact]
    public void GetFloat_MissingOrInvalid_ReturnsNull()
    {
        var node = CreateComponent("m_Value: notANumber");

        Assert.Null(node.GetFloat("m_Value"));
        Assert.Null(node.GetFloat("Missing"));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("FALSE", false)]
    public void GetBool_ReturnsExpectedValue(string input, bool expected)
    {
        var node = CreateComponent($"m_Enabled: {input}");

        Assert.Equal(expected, node.GetBool("m_Enabled"));
    }

    [Fact]
    public void GetBool_MissingOrInvalid_ReturnsNull()
    {
        var node = CreateComponent("m_Enabled: maybe");

        Assert.Null(node.GetBool("m_Enabled"));
        Assert.Null(node.GetBool("Missing"));
    }

    [Fact]
    public void GetReference_ReturnsFileIdAndGuid()
    {
        var node = CreateComponent("m_Target:\n  fileID: 1234\n  guid: a1b2c3d4");

        var reference = node.GetReference("m_Target");

        Assert.NotNull(reference);
        Assert.Equal(1234L, reference.Value.FileID);
        Assert.Equal("a1b2c3d4", reference.Value.Guid);
    }

    [Fact]
    public void GetReference_WithoutGuid_ReturnsNullGuid()
    {
        var node = CreateComponent("m_Target:\n  fileID: 5678");

        var reference = node.GetReference("m_Target");

        Assert.NotNull(reference);
        Assert.Equal(5678L, reference.Value.FileID);
        Assert.Null(reference.Value.Guid);
    }

    [Fact]
    public void GetReference_MissingKey_ReturnsNull()
    {
        var node = CreateComponent("m_Target:\n  fileID: 1234");

        Assert.Null(node.GetReference("Missing"));
    }

    [Fact]
    public void GetColor_ReturnsColor()
    {
        var node = CreateComponent("m_Color:\n  r: 1\n  g: 0\n  b: 0\n  a: 1");

        var color = node.GetColor("m_Color");

        Assert.True(color.HasValue);
        Assert.Equal(1.0, (double)color.Value.R, 4);
        Assert.Equal(0.0, (double)color.Value.G, 4);
        Assert.Equal(0.0, (double)color.Value.B, 4);
        Assert.Equal(1.0, (double)color.Value.A, 4);
    }

    [Fact]
    public void GetColor_MissingOrInvalid_ReturnsNull()
    {
        var node = CreateComponent("m_Color:\n  r: 1");

        Assert.Null(node.GetColor("m_Color"));
        Assert.Null(node.GetColor("Missing"));
    }

    [Fact]
    public void GetVector2_ReturnsVector()
    {
        var node = CreateComponent("m_SizeDelta:\n  x: 100\n  y: 50");

        var vector = node.GetVector2("m_SizeDelta");

        Assert.True(vector.HasValue);
        Assert.Equal(100.0, (double)vector.Value.X, 4);
        Assert.Equal(50.0, (double)vector.Value.Y, 4);
    }

    [Fact]
    public void GetVector2_MissingOrInvalid_ReturnsNull()
    {
        var node = CreateComponent("m_SizeDelta:\n  x: 100");

        Assert.Null(node.GetVector2("m_SizeDelta"));
        Assert.Null(node.GetVector2("Missing"));
    }

    private static ComponentNode CreateComponent(string yaml)
    {
        using var reader = new StringReader(yaml);
        var stream = new YamlStream();
        stream.Load(reader);
        var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
        return new ComponentNode(1, 1, "Test", mapping);
    }
}
