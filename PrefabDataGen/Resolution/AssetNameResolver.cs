namespace EmberConfig.PrefabDataGen.Resolution;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

internal sealed class AssetNameResolver
{
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
        var assetsPath = assetRipsPath;
        if (!Directory.Exists(assetsPath))
            return index;

        foreach (var metaPath in Directory.EnumerateFiles(assetsPath, "*.meta", SearchOption.AllDirectories))
        {
            var guid = ExtractGuid(metaPath);
            if (string.IsNullOrEmpty(guid))
                continue;

            var assetPath = metaPath[..^".meta".Length];
            if (File.Exists(assetPath))
                index[guid] = assetPath;
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
            var regex = new Regex(@"^\s*m_Name:\s*(.*)$", RegexOptions.Compiled);
            foreach (var line in File.ReadLines(assetPath).Take(30))
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var name = match.Groups[1].Value.Trim();
                    return string.IsNullOrEmpty(name) ? null : name;
                }
            }
        }
        catch
        {
            // ignored
        }

        return Path.GetFileNameWithoutExtension(assetPath);
    }
}
