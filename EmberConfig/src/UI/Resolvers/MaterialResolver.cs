namespace EmberConfig.UI;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class MaterialResolver
{
    private static readonly Dictionary<string, Material?> Cache = new();
    private static Material[]? loadedMaterials;

    internal static Material? Resolve(string? name)
    {
        if (Cache.TryGetValue(name ?? string.Empty, out var cached))
            return cached;

        loadedMaterials ??= UnityEngine.Resources.FindObjectsOfTypeAll<Material>();
        if (loadedMaterials is null || loadedMaterials.Length == 0)
        {
            // Don't cache an empty scan; materials may still be loading.
            loadedMaterials = null;
            return null;
        }

        if (!string.IsNullOrEmpty(name))
        {
            var material = loadedMaterials.FirstOrDefault(m => m.name == name);
            if (material is not null)
            {
                Cache[name] = material;
                return material;
            }
        }

        Cache[name ?? string.Empty] = null;
        return null;
    }
}
