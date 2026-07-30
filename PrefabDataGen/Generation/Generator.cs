namespace EmberConfig.PrefabDataGen.Generation;

using System;
using System.IO;
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
        var panelPrefabPath = Path.Combine(paths.ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting", "PC_Panel_setting.prefab");
        var dropdownPrefabPath = Path.Combine(paths.ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting", "SettingDropdown_PCunit.prefab");
        var switchPrefabPath = Path.Combine(paths.ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting", "SettingClickGroup_PCunit.prefab");
        var sliderPrefabPath = Path.Combine(paths.ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting", "SettingSlider_PCunit.prefab");

        if (!File.Exists(panelPrefabPath))
            throw new FileNotFoundException($"Panel prefab not found: {panelPrefabPath}");

        Console.WriteLine($"Loading prefab: {panelPrefabPath}");
        var document = UnityPrefabLoader.Load(panelPrefabPath);
        Console.WriteLine($"  -> {document.GameObjects.Count} GameObjects, {document.Components.Count} components");

        var row = RowStyleExtractor.Extract(document, assetNameResolver);

        DropdownRawStyle? dropdown = null;
        if (File.Exists(dropdownPrefabPath))
        {
            Console.WriteLine($"Loading dropdown prefab: {dropdownPrefabPath}");
            var dropdownDocument = UnityPrefabLoader.Load(dropdownPrefabPath);
            Console.WriteLine($"  -> {dropdownDocument.GameObjects.Count} GameObjects, {dropdownDocument.Components.Count} components");
            dropdown = DropdownStyleExtractor.Extract(dropdownDocument, assetNameResolver);
        }
        else
        {
            Console.WriteLine($"Dropdown prefab not found: {dropdownPrefabPath}");
        }

        SwitchRawStyle? switchStyle = null;
        if (File.Exists(switchPrefabPath))
        {
            Console.WriteLine($"Loading switch prefab: {switchPrefabPath}");
            var switchDocument = UnityPrefabLoader.Load(switchPrefabPath);
            Console.WriteLine($"  -> {switchDocument.GameObjects.Count} GameObjects, {switchDocument.Components.Count} components");
            switchStyle = SwitchStyleExtractor.Extract(switchDocument, assetNameResolver);
        }
        else
        {
            Console.WriteLine($"Switch prefab not found: {switchPrefabPath}");
        }

        var outputDir = paths.OutputPath;
        Directory.CreateDirectory(outputDir);

        if (dropdown is null)
            throw new InvalidOperationException("Dropdown style could not be extracted.");

        if (switchStyle is null)
            throw new InvalidOperationException("Switch style could not be extracted.");

        SliderRawStyle? slider = null;
        if (File.Exists(sliderPrefabPath))
        {
            Console.WriteLine($"Loading slider prefab: {sliderPrefabPath}");
            var sliderDocument = UnityPrefabLoader.Load(sliderPrefabPath);
            Console.WriteLine($"  -> {sliderDocument.GameObjects.Count} GameObjects, {sliderDocument.Components.Count} components");
            slider = SliderStyleExtractor.Extract(sliderDocument, assetNameResolver);
        }
        else
        {
            Console.WriteLine($"Slider prefab not found: {sliderPrefabPath}");
        }

        if (slider is null)
            throw new InvalidOperationException("Slider style could not be extracted.");

        var legacyFactoryPath = Path.Combine(outputDir, "PrefabStyleFactory.cs");
        if (File.Exists(legacyFactoryPath))
            File.Delete(legacyFactoryPath);

        var rowFactoryPath = Path.Combine(outputDir, "RowStyleFactory.cs");
        var dropdownFactoryPath = Path.Combine(outputDir, "DropdownStyleFactory.cs");
        var switchFactoryPath = Path.Combine(outputDir, "SwitchStyleFactory.cs");
        var sliderFactoryPath = Path.Combine(outputDir, "SliderStyleFactory.cs");
        CSharpFileWriter.WriteRowStyleFactory(rowFactoryPath, row);
        CSharpFileWriter.WriteDropdownStyleFactory(dropdownFactoryPath, dropdown);
        CSharpFileWriter.WriteSwitchStyleFactory(switchFactoryPath, switchStyle);
        CSharpFileWriter.WriteSliderStyleFactory(sliderFactoryPath, slider);
        Console.WriteLine($"Wrote {rowFactoryPath}");
        Console.WriteLine($"Wrote {dropdownFactoryPath}");
        Console.WriteLine($"Wrote {switchFactoryPath}");
        Console.WriteLine($"Wrote {sliderFactoryPath}");
    }
}
