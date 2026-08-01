namespace EmberConfig.PrefabDataGen.Resolution;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EmberConfig.PrefabDataGen;

internal sealed class AssetNameResolver
{
    private static readonly Regex AssetNameRegex = new(@"^\s*m_Name:\s*(.*)$", RegexOptions.Compiled);

    private readonly string assetRipsPath;
    private readonly Dictionary<string, string> guidToAssetPath;

    public AssetNameResolver(string assetRipsPath)
    {
        this.assetRipsPath = assetRipsPath;
        guidToAssetPath = BuildGuidIndex();
    }

    public string? ResolveName(string? guid)
    {
        if (string.IsNullOrEmpty(guid))
            return null;

        if (!guidToAssetPath.TryGetValue(guid, out var assetPath))
            return null;

        return ReadAssetName(assetPath);
    }

    private Dictionary<string, string> BuildGuidIndex()
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(assetRipsPath))
            return index;

        var extensions = new[] { "*.asset", "*.mat", "*.png", "*.jpg", "*.tga", "*.psd" };
        foreach (var extension in extensions)
        {
            foreach (var assetPath in Directory.EnumerateFiles(assetRipsPath, extension, SearchOption.AllDirectories))
            {
                var metaPath = assetPath + ".meta";
                if (!File.Exists(metaPath))
                    continue;

                var guid = ExtractGuid(metaPath);
                if (!string.IsNullOrEmpty(guid))
                    index[guid] = assetPath;
            }
        }

        return index;
    }

    private static string? ExtractGuid(string metaPath)
    {
        foreach (var line in File.ReadLines(metaPath).Take(20))
        {
            const string prefix = "guid:";
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var start = line.IndexOf(':');
                if (start >= 0)
                    return line[(start + 1)..].Trim();
            }
        }

        return null;
    }

    private static string? ReadAssetName(string assetPath)
    {
        if (!File.Exists(assetPath))
            return null;

        try
        {
            foreach (var line in File.ReadLines(assetPath).Take(30))
            {
                var match = AssetNameRegex.Match(line);
                if (match.Success)
                {
                    var name = match.Groups[1].Value.Trim();
                    return string.IsNullOrEmpty(name) ? null : name;
                }
            }
        }
        catch (IOException ex)
        {
            Log.Error($"Could not read asset name from {assetPath}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error($"Could not read asset name from {assetPath}: {ex.Message}");
        }

        return Path.GetFileNameWithoutExtension(assetPath);
    }
}
