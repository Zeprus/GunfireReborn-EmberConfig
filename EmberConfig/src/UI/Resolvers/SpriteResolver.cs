namespace EmberConfig.UI;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class SpriteResolver
{
    private static readonly Dictionary<string, Sprite?> Cache = new();
    private static Sprite[]? loadedSprites;

    internal static Sprite? Resolve(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (Cache.TryGetValue(name, out var cached))
            return cached;

        loadedSprites ??= UnityEngine.Resources.FindObjectsOfTypeAll<Sprite>();
        if (loadedSprites is null || loadedSprites.Length == 0)
        {
            // Don't cache an empty scan; sprites may still be loading.
            loadedSprites = null;
            return null;
        }

        var sprite = loadedSprites.FirstOrDefault(s => s.name == name);
        Cache[name] = sprite;
        return sprite;
    }
}
