namespace EmberConfig.PrefabDataGen.Generation;

using System;
using System.Collections.Generic;
using System.IO;
using EmberConfig.PrefabDataGen;
using EmberConfig.PrefabDataGen.Configuration;
using EmberConfig.PrefabDataGen.Extraction;
using EmberConfig.PrefabDataGen.Models;
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
        var panelPrefabPath = Path.Combine(paths.PrefabsPath, "PC_Panel_setting.prefab");

        if (!File.Exists(panelPrefabPath))
            throw new InvalidOperationException($"Panel prefab not found: {panelPrefabPath}");

        Log.Info($"Loading prefab: {panelPrefabPath}");
        var document = UnityPrefabLoader.Load(panelPrefabPath);
        Log.Info($"  -> {document.GameObjects.Count} GameObjects, {document.Components.Count} components");

        var row = RowStyleExtractor.Extract(document, assetNameResolver);
        var tab = TabStyleExtractor.Extract(document, assetNameResolver);

        var outputDir = paths.OutputPath;
        Directory.CreateDirectory(outputDir);

        var jobs = new List<IStyleJob>
        {
            new StyleJob<DropdownRawStyle>(
                "SettingDropdown_PCunit.prefab",
                true,
                DropdownStyleExtractor.Extract,
                CSharpFileWriter.WriteDropdownStyleFactory,
                "DropdownStyleFactory.cs"),
            new StyleJob<SwitchRawStyle>(
                "SettingClickGroup_PCunit.prefab",
                true,
                SwitchStyleExtractor.Extract,
                CSharpFileWriter.WriteSwitchStyleFactory,
                "SwitchStyleFactory.cs"),
            new StyleJob<SliderRawStyle>(
                "SettingSlider_PCunit.prefab",
                true,
                SliderStyleExtractor.Extract,
                CSharpFileWriter.WriteSliderStyleFactory,
                "SliderStyleFactory.cs"),
            new StyleJob<KeybindButtonRawStyle>(
                "SettingKeyChange_PCunit.prefab",
                true,
                KeybindButtonStyleExtractor.Extract,
                CSharpFileWriter.WriteKeybindButtonStyleFactory,
                "KeybindButtonStyleFactory.cs"),
            new StyleJob<CarouselRawStyle>(
                "SettingMutiClickGrop_PCunitGraphic.prefab",
                true,
                CarouselStyleExtractor.Extract,
                CSharpFileWriter.WriteCarouselStyleFactory,
                "CarouselStyleFactory.cs"),
            new StyleJob<object?>(
                string.Empty,
                false,
                (_, _) => null,
                (path, _) => CSharpFileWriter.WriteInputStyleFactory(path),
                "InputStyleFactory.cs")
        };

        var rowFactoryPath = Path.Combine(outputDir, "RowStyleFactory.cs");
        var tabFactoryPath = Path.Combine(outputDir, "TabStyleFactory.cs");

        CSharpFileWriter.WriteRowStyleFactory(rowFactoryPath, row);
        Log.Info($"Wrote {rowFactoryPath}");

        foreach (var job in jobs)
            job.Run(assetNameResolver, paths.PrefabsPath, outputDir);

        CSharpFileWriter.WriteTabStyleFactory(tabFactoryPath, tab);
        Log.Info($"Wrote {tabFactoryPath}");
    }

    private interface IStyleJob
    {
        void Run(AssetNameResolver resolver, string prefabsPath, string outputDir);
    }

    private sealed record StyleJob<TStyle>(
        string PrefabFileName,
        bool Required,
        Func<PrefabDocument, AssetNameResolver, TStyle> Extract,
        Action<string, TStyle> Write,
        string OutputFileName) : IStyleJob
    {
        public void Run(AssetNameResolver resolver, string prefabsPath, string outputDir)
        {
            TStyle? style = default;

            if (!string.IsNullOrEmpty(PrefabFileName))
            {
                var path = Path.Combine(prefabsPath, PrefabFileName);
                if (!File.Exists(path))
                {
                    if (Required)
                        throw new InvalidOperationException($"Required prefab not found: {path}");

                    Log.Info($"Optional prefab not found: {path}");
                    return;
                }

                Log.Info($"Loading prefab: {path}");
                var document = UnityPrefabLoader.Load(path);
                Log.Info($"  -> {document.GameObjects.Count} GameObjects, {document.Components.Count} components");

                style = Extract(document, resolver);
            }

            if (style is null && Required)
                throw new InvalidOperationException($"Extraction failed for {OutputFileName}");

            var outputPath = Path.Combine(outputDir, OutputFileName);
            Write(outputPath, style!);
            Log.Info($"Wrote {outputPath}");
        }
    }
}
