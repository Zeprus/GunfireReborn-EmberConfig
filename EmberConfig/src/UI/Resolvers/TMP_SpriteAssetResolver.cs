namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

internal static class TMP_SpriteAssetResolver
{
    private static readonly Dictionary<string, TMP_SpriteAsset> Cache = new();
    private static TMP_SpriteAsset[]? loadedAssets;
    private static TMP_SpriteAsset? anyAsset;

    internal static TMP_SpriteAsset? Resolve(string? name)
    {
        if (Cache.TryGetValue(name ?? string.Empty, out var cached))
            return cached;

        loadedAssets ??= UnityEngine.Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
        anyAsset ??= loadedAssets.FirstOrDefault();

        if (!string.IsNullOrEmpty(name))
        {
            var asset = loadedAssets.FirstOrDefault(s => s.name == name);
            if (asset is not null)
            {
                Cache[name] = asset;
                return asset;
            }
        }

        if (anyAsset is not null)
        {
            Cache[name ?? string.Empty] = anyAsset;
            return anyAsset;
        }

        return null;
    }
}
