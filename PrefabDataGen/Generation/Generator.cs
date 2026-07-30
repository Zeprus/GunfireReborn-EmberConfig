namespace EmberConfig.PrefabDataGen.Generation;

using System;
using System.IO;
using EmberConfig.PrefabDataGen.Configuration;
using EmberConfig.PrefabDataGen.Extraction;
using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;

internal sealed class Generator
{
    private readonly Paths paths;

    public Generator(Paths paths)
    {
        this.paths = paths;
    }

    public void Run()
    {
        var assetNameResolver = new AssetNameResolver(paths.ExportedProjectAssetsPath);
        var panelPrefabPath = Path.Combine(paths.ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting", "PC_Panel_setting.prefab");

        if (!File.Exists(panelPrefabPath))
            throw new FileNotFoundException($"Panel prefab not found: {panelPrefabPath}");

        Console.WriteLine($"Loading prefab: {panelPrefabPath}");
        var document = UnityPrefabLoader.Load(panelPrefabPath);
        Console.WriteLine($"  -> {document.GameObjects.Count} GameObjects, {document.Components.Count} components");

        var row = RowStyleExtractor.Extract(document, assetNameResolver);

        var outputDir = paths.OutputPath;
        Directory.CreateDirectory(outputDir);

        var factoryPath = Path.Combine(outputDir, "PrefabStyleFactory.cs");
        CSharpFileWriter.WriteStyleFactory(factoryPath, row);
        Console.WriteLine($"Wrote {factoryPath}");
    }
}
