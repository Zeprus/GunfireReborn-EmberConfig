namespace EmberConfig.PrefabDataGen.Configuration;

using System.IO;

internal sealed class Paths
{
    public string AssetRipsPath { get; }
    public string OutputPath { get; }
    public string ExportedProjectAssetsPath => Path.Combine(AssetRipsPath, "Assets");
    public string PrefabsPath => Path.Combine(ExportedProjectAssetsPath, "res", "uisteam", "panel_prefabs", "setting");

    public Paths(string assetRipsPath, string outputPath)
    {
        AssetRipsPath = assetRipsPath;
        OutputPath = outputPath;
    }
}
