namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

internal static class TMP_FontAssetResolver
{
    private static readonly Dictionary<string, TMP_FontAsset> Cache = new();
    private static TMP_FontAsset[]? loadedFonts;
    private static TMP_FontAsset? anyFont;

    internal static TMP_FontAsset Resolve(string? name)
    {
        if (Cache.TryGetValue(name ?? string.Empty, out var cached))
            return cached;

        loadedFonts ??= UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
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

        if (anyFont is not null)
        {
            Cache[name ?? string.Empty] = anyFont;
            return anyFont;
        }

        throw new InvalidOperationException("No TMP_FontAsset found in loaded resources.");
    }
}
