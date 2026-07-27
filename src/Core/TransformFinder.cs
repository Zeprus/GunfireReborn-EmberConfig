namespace SettingsLib.Core;

using System;
using UnityEngine;

internal static class TransformFinder
{
    internal static Transform? Find(Transform? root, string name, bool recursive = true)
    {
        if (root is null)
            return null;

        var direct = root.Find(name);
        if (direct is not null)
            return direct;

        return recursive ? FindRecursive(root, name, StringComparison.OrdinalIgnoreCase, partial: false) : null;
    }

    internal static Transform? FindByPartialName(Transform? root, string name)
    {
        if (root is null)
            return null;

        var direct = root.Find(name);
        if (direct is not null)
            return direct;

        return FindRecursive(root, name, StringComparison.OrdinalIgnoreCase, partial: true);
    }

    private static Transform? FindRecursive(Transform root, string name, StringComparison comparison, bool partial)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child is null)
                continue;

            if (NameMatches(child.name, name, comparison, partial))
                return child;

            var found = FindRecursive(child, name, comparison, partial);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static bool NameMatches(string candidate, string name, StringComparison comparison, bool partial)
    {
        return partial
            ? candidate.Contains(name, comparison)
            : string.Equals(candidate, name, comparison);
    }
}
