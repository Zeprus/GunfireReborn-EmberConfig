namespace EmberConfig.PrefabDataGen.Tests;

using System.Collections.Generic;
using System.Linq;
using EmberConfig.PrefabDataGen.Parsing;
using Xunit;

public class GameObjectNodeTests
{
    [Fact]
    public void FindChild_ReturnsMatchingChild()
    {
        var root = new GameObjectNode(1, "Root", true, new List<ComponentNode>());
        var child = new GameObjectNode(2, "Child", true, new List<ComponentNode>());
        root.Children.Add(child);

        Assert.Same(child, root.FindChild("Child"));
        Assert.Null(root.FindChild("Missing"));
    }

    [Fact]
    public void FindDescendants_ReturnsDirectAndNestedMatches()
    {
        var root = new GameObjectNode(1, "Root", true, new List<ComponentNode>());
        var a = new GameObjectNode(2, "A", true, new List<ComponentNode>());
        var c = new GameObjectNode(3, "C", true, new List<ComponentNode>());
        var b = new GameObjectNode(4, "A", true, new List<ComponentNode>());

        root.Children.Add(a);
        a.Children.Add(c);
        c.Children.Add(b);

        var descendants = root.FindDescendants("A").ToList();

        Assert.Equal(2, descendants.Count);
        Assert.Contains(a, descendants);
        Assert.Contains(b, descendants);
    }

    [Fact]
    public void FindDescendants_DoesNotReturnUnmatchedDescendants()
    {
        var root = new GameObjectNode(1, "Root", true, new List<ComponentNode>());
        var other = new GameObjectNode(2, "Other", true, new List<ComponentNode>());
        root.Children.Add(other);

        var descendants = root.FindDescendants("A").ToList();

        Assert.Empty(descendants);
    }
}
