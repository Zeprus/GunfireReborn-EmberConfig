namespace EmberConfig.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class MaterialResolver
{
    private static readonly Dictionary<string, Material> Cache = new();
    private static Material[]? loadedMaterials;
    private static Material? anyMaterial;

    internal static Material Resolve(string? name)
    {
        if (Cache.TryGetValue(name ?? string.Empty, out var cached))
            return cached;

        loadedMaterials ??= UnityEngine.Resources.FindObjectsOfTypeAll<Material>();
        anyMaterial ??= loadedMaterials.FirstOrDefault();

        if (!string.IsNullOrEmpty(name))
        {
            var material = loadedMaterials.FirstOrDefault(m => m.name == name);
            if (material is not null)
            {
                Cache[name] = material;
                return material;
            }
        }

        if (anyMaterial is not null)
        {
            Cache[name ?? string.Empty] = anyMaterial;
            return anyMaterial;
        }

        throw new InvalidOperationException("No Material found in loaded resources.");
    }
}
