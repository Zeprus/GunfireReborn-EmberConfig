namespace EmberConfig.PrefabDataGen.Parsing;

using System.Linq;
using YamlDotNet.RepresentationModel;

internal static class YamlMappingNodeExtensions
{
    internal static bool TryGetChild(this YamlMappingNode? mapping, string key, out YamlNode? node)
    {
        if (mapping is not null)
        {
            foreach (var kvp in mapping.Children)
            {
                if (kvp.Key is YamlScalarNode scalar &&
                    string.Equals(scalar.Value, key, System.StringComparison.Ordinal))
                {
                    node = kvp.Value;
                    return true;
                }
            }
        }

        node = null;
        return false;
    }
}
