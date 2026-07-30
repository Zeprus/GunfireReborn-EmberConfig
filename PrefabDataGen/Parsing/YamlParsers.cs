namespace EmberConfig.PrefabDataGen.Parsing;

using System;
using System.Globalization;
using EmberConfig.PrefabDataGen.Models;
using YamlDotNet.RepresentationModel;

internal static class YamlParsers
{
    internal static Color? ParseColor(YamlMappingNode? mapping)
    {
        if (mapping is null)
            return null;

        if (mapping.TryGetChild("rgba", out var rgbaNode) &&
            rgbaNode is YamlScalarNode rgbaScalar &&
            uint.TryParse(rgbaScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rgba))
        {
            return new Color(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f);
        }

        if (mapping.TryGetChild("r", out var rNode) && rNode is YamlScalarNode rScalar &&
            float.TryParse(rScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) &&
            mapping.TryGetChild("g", out var gNode) && gNode is YamlScalarNode gScalar &&
            float.TryParse(gScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var g) &&
            mapping.TryGetChild("b", out var bNode) && bNode is YamlScalarNode bScalar &&
            float.TryParse(bScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var b) &&
            mapping.TryGetChild("a", out var aNode) && aNode is YamlScalarNode aScalar &&
            float.TryParse(aScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var a))
        {
            return new Color(r, g, b, a);
        }

        return null;
    }

    internal static Vector2? ParseVector2(YamlMappingNode? mapping)
    {
        if (mapping is null)
            return null;

        if (mapping.TryGetChild("x", out var xNode) && xNode is YamlScalarNode xScalar &&
            float.TryParse(xScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
            mapping.TryGetChild("y", out var yNode) && yNode is YamlScalarNode yScalar &&
            float.TryParse(yScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
        {
            return new Vector2(x, y);
        }

        return null;
    }

    internal static Vector3? ParseVector3(YamlMappingNode? mapping)
    {
        if (mapping is null)
            return null;

        if (mapping.TryGetChild("x", out var xNode) && xNode is YamlScalarNode xScalar &&
            float.TryParse(xScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
            mapping.TryGetChild("y", out var yNode) && yNode is YamlScalarNode yScalar &&
            float.TryParse(yScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
            mapping.TryGetChild("z", out var zNode) && zNode is YamlScalarNode zScalar &&
            float.TryParse(zScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var z))
        {
            return new Vector3(x, y, z);
        }

        return null;
    }

    internal static float? ParseFloat(YamlScalarNode? node) =>
        node is not null && float.TryParse(node.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    internal static int? ParseInt(YamlScalarNode? node) =>
        node is not null && int.TryParse(node.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    internal static string? ParseString(YamlScalarNode? node) => node?.Value;
}
