namespace EmberConfig.PrefabDataGen.Extraction;

using System.Globalization;
using EmberConfig.PrefabDataGen.Parsing;
using YamlDotNet.RepresentationModel;

internal static class PaddingExtractor
{
    internal static int GetPadding(ComponentNode? layout, string key)
    {
        var paddingNode = layout?.GetMapping("m_Padding");
        if (paddingNode is null)
            return 0;

        if (!paddingNode.TryGetChild(key, out var node) || node is not YamlScalarNode scalar)
            return 0;

        return int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }
}
