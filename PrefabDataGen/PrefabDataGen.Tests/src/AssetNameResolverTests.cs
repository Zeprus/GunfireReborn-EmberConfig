namespace EmberConfig.PrefabDataGen.Tests;

using System;
using System.IO;
using EmberConfig.PrefabDataGen.Resolution;
using Xunit;

public class AssetNameResolverTests : IDisposable
{
    private readonly string tempDir;

    public AssetNameResolverTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var spritesDir = Path.Combine(tempDir, "res", "uisteam", "sprites");
        Directory.CreateDirectory(spritesDir);

        var assetPath = Path.Combine(spritesDir, "MySprite.asset");
        File.WriteAllText(assetPath, "%YAML 1.1\n---\nGameObject:\n  m_Name: MySprite\n");
        File.WriteAllText(assetPath + ".meta", "fileFormatVersion: 2\nguid: a1b2c3d4e5f6\n");

        var materialPath = Path.Combine(spritesDir, "MyMaterial.mat");
        File.WriteAllText(materialPath, "%YAML 1.1\n---\nMaterial:\n  m_Name: MyMaterial\n");
        File.WriteAllText(materialPath + ".meta", "fileFormatVersion: 2\nguid: 112233445566\n");
    }

    [Fact]
    public void ResolveName_AssetAndMaterial_ReturnsExpectedNames()
    {
        var resolver = new AssetNameResolver(tempDir);

        Assert.Equal("MySprite", resolver.ResolveName("a1b2c3d4e5f6"));
        Assert.Equal("MyMaterial", resolver.ResolveName("112233445566"));
    }

    [Fact]
    public void ResolveName_UnknownGuid_ReturnsNull()
    {
        var resolver = new AssetNameResolver(tempDir);

        Assert.Null(resolver.ResolveName("unknown-guid"));
        Assert.Null(resolver.ResolveName(null));
        Assert.Null(resolver.ResolveName(string.Empty));
    }

    [Fact]
    public void ResolveName_AssetWithoutMeta_IsNotIndexed()
    {
        var unindexedDir = Path.Combine(tempDir, "unindexed");
        Directory.CreateDirectory(unindexedDir);
        var assetPath = Path.Combine(unindexedDir, "NoMeta.asset");
        File.WriteAllText(assetPath, "%YAML 1.1\n---\nGameObject:\n  m_Name: NoMeta\n");

        var resolver = new AssetNameResolver(tempDir);

        Assert.Null(resolver.ResolveName("missing-meta"));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(tempDir, true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
