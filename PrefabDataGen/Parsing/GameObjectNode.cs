namespace EmberConfig.PrefabDataGen.Parsing;

using System.Collections.Generic;
using System.Linq;

internal sealed class GameObjectNode
{
    public long FileID { get; }
    public string Name { get; }
    public bool IsActive { get; }
    public ComponentNode? RectTransform { get; set; }
    public List<ComponentNode> Components { get; }
    public GameObjectNode? Parent { get; set; }
    public List<GameObjectNode> Children { get; } = new();

    public GameObjectNode(long fileID, string name, bool isActive, List<ComponentNode> components)
    {
        FileID = fileID;
        Name = name;
        IsActive = isActive;
        Components = components;
    }

    public ComponentNode? GetComponentByTypeName(string typeName) =>
        Components.FirstOrDefault(c => c.TypeName == typeName);

    public GameObjectNode? FindChild(string name) =>
        Children.FirstOrDefault(c => c.Name == name);

    public IEnumerable<GameObjectNode> FindDescendants(string name) =>
        Children.SelectMany(c => c.FindDescendants(name).Prepend(c)).Where(c => c.Name == name);
}
