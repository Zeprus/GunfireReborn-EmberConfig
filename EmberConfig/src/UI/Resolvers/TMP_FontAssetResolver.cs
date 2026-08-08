namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

internal static class TMP_FontAssetResolver
{
    private static readonly Dictionary<string, TMP_FontAsset?> Cache = new();
    private static TMP_FontAsset[]? loadedFonts;
    private static TMP_FontAsset? anyFont;

    internal static TMP_FontAsset? Resolve(string? name)
    {
        if (Cache.TryGetValue(name ?? string.Empty, out var cached))
            return cached;

        // Avoid caching an empty result: resources may still be loading,
        // so keep querying until at least one font is available.
        loadedFonts ??= UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (loadedFonts is null || loadedFonts.Length == 0)
        {
            loadedFonts = null;
            anyFont = null;
            return null;
        }

        anyFont ??= loadedFonts.FirstOrDefault();

        if (!string.IsNullOrEmpty(name))
        {
            var font = loadedFonts.FirstOrDefault(f => f.name == name);
            if (font is not null)
            {
                Cache[name] = font;
                return font;
            }
        }

        // Named font not found; fall back to the first loaded font instead of
        // throwing. The caller can detect a null font and retry initialization.
        Cache[name ?? string.Empty] = anyFont;
        return anyFont;
    }
}
