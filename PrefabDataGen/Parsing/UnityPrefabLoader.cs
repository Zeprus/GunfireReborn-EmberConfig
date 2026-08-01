namespace EmberConfig.PrefabDataGen.Parsing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmberConfig.PrefabDataGen;
using YamlDotNet.RepresentationModel;

internal static class UnityPrefabLoader
{
    internal static PrefabDocument Load(string path)
    {
        Log.Info($"  [Loader] reading {path} ...");
        using var reader = new StreamReader(path);
        var stream = new YamlStream();
        Log.Info($"  [Loader] parsing YAML ...");
        stream.Load(reader);
        Log.Info($"  [Loader] parsed {stream.Documents.Count} YAML documents");

        var components = new Dictionary<long, ComponentNode>();
        var gameObjects = new Dictionary<long, GameObjectNode>();

        Log.Info($"  [Loader] building component / GameObject index ...");

        foreach (var document in stream.Documents)
        {
            var root = document.RootNode;
            if (root is not YamlMappingNode mapping)
                continue;

            var anchor = root.Anchor.Value;
            if (string.IsNullOrEmpty(anchor) || !long.TryParse(anchor, out var fileID))
                continue;

            if (mapping.Children.Count == 0)
            {
                Log.Error($"Skipping empty YAML mapping in {path} with anchor '{anchor}'");
                continue;
            }

            var typeEntry = mapping.Children.First();
            if (typeEntry.Key is not YamlScalarNode keyScalar)
            {
                Log.Error($"Skipping YAML mapping with non-scalar first key in {path} (anchor '{anchor}')");
                continue;
            }

            var typeName = keyScalar.Value ?? string.Empty;
            var innerProperties = typeEntry.Value as YamlMappingNode ?? mapping;
            var unityType = ParseUnityType(root.Tag.Value);
            var component = new ComponentNode(fileID, unityType, typeName, innerProperties);
            components[fileID] = component;

            if (unityType == 1)
            {
                var name = component.GetString("m_Name") ?? string.Empty;
                var isActive = component.GetInt("m_IsActive") is 1;
                gameObjects[fileID] = new GameObjectNode(fileID, name, isActive, new List<ComponentNode>());
            }
        }

        // Associate components with GameObjects and link RectTransform hierarchy.
        var rectTransformMap = new Dictionary<long, long>(); // rectTransform fileID -> gameObject fileID
        var childToParent = new Dictionary<long, long>();    // child rectTransform fileID -> parent rectTransform fileID

        foreach (var component in components.Values)
        {
            if (component.UnityType == 224)
            {
                var goRef = component.GetReference("m_GameObject");
                if (goRef.HasValue && gameObjects.TryGetValue(goRef.Value.FileID, out var goNode))
                {
                    goNode.Components.Add(component);
                    goNode.RectTransform = component;
                    rectTransformMap[component.FileID] = goRef.Value.FileID;
                }

                var fatherRef = component.GetReference("m_Father");
                if (fatherRef.HasValue && fatherRef.Value.FileID != 0)
                    childToParent[component.FileID] = fatherRef.Value.FileID;
            }
            else
            {
                var goRef = component.GetReference("m_GameObject");
                if (goRef.HasValue && gameObjects.TryGetValue(goRef.Value.FileID, out var goNode))
                {
                    goNode.Components.Add(component);
                }
            }
        }

        // Build parent/child relationships.
        foreach (var childKvp in childToParent)
        {
            if (!rectTransformMap.TryGetValue(childKvp.Key, out var childGoFileID))
                continue;
            if (!rectTransformMap.TryGetValue(childKvp.Value, out var parentGoFileID))
                continue;

            if (gameObjects.TryGetValue(childGoFileID, out var childGo) &&
                gameObjects.TryGetValue(parentGoFileID, out var parentGo))
            {
                childGo.Parent = parentGo;
                parentGo.Children.Add(childGo);
            }
        }

        // Root order by RectTransform children order.
        Log.Info($"  [Loader] wiring hierarchy ...");

        foreach (var component in components.Values.Where(c => c.UnityType == 224))
        {
            if (!rectTransformMap.TryGetValue(component.FileID, out var goFileID))
                continue;
            if (!gameObjects.TryGetValue(goFileID, out var go))
                continue;

            var orderedChildren = component.GetFileIDList("m_Children")
                .Where(rectTransformMap.ContainsKey)
                .Select(id => gameObjects[rectTransformMap[id]])
                .ToList();

            go.Children.Clear();
            go.Children.AddRange(orderedChildren);
        }

        Log.Info($"  [Loader] done: {gameObjects.Count} GameObjects, {components.Count} components");
        return new PrefabDocument(components, gameObjects);
    }

    private static int ParseUnityType(string? tag)
    {
        if (string.IsNullOrEmpty(tag))
            return 0;

        // tag looks like "tag:unity3d.com,2011:1" or "!u!1"
        var last = tag.Split(':', '!').Last();
        if (int.TryParse(last, out var type))
            return type;

        return 0;
    }
}
