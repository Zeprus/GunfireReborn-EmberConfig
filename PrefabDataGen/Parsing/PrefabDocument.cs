namespace EmberConfig.PrefabDataGen.Parsing;

using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class PrefabDocument
{
    public IReadOnlyDictionary<long, ComponentNode> Components { get; }
    public IReadOnlyDictionary<long, GameObjectNode> GameObjects { get; }
    public IReadOnlyList<GameObjectNode> RootGameObjects { get; }

    public PrefabDocument(IReadOnlyDictionary<long, ComponentNode> components, IReadOnlyDictionary<long, GameObjectNode> gameObjects)
    {
        Components = components;
        GameObjects = gameObjects;
        RootGameObjects = gameObjects.Values.Where(g => g.Parent is null).ToList();
    }

    public GameObjectNode? GetGameObject(long fileID) =>
        GameObjects.TryGetValue(fileID, out var node) ? node : null;

    public GameObjectNode? FindBestRow()
    {
        var candidates = GameObjects.Values
            .Where(g => g.Children.Any(c => c.Name == "Title") && g.Children.Any(c => c.Name == "Item"))
            .ToList();

        if (candidates.Count == 0)
            return null;

        GameObjectNode? best = null;
        float bestScore = float.MaxValue;

        foreach (var candidate in candidates)
        {
            var item = candidate.FindChild("Item")!;
            var hasSlider = item.FindChild("Slider_PCunit") is not null;

            float height = candidate.RectTransform?.GetVector2("m_SizeDelta")?.Y ?? 0f;
            float itemHeight = item.RectTransform?.GetVector2("m_SizeDelta")?.Y ?? height;

            float score = MathF.Abs(height - 50f) * 2f
                        + MathF.Abs(itemHeight - 50f) * 2f
                        + (candidate.IsActive ? 0f : 500f)
                        + (hasSlider ? -100f : 0f);

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best ?? candidates[0];
    }
}
