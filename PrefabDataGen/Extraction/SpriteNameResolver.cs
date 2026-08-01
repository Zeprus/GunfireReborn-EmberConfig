namespace EmberConfig.PrefabDataGen.Extraction;

using EmberConfig.PrefabDataGen.Parsing;
using EmberConfig.PrefabDataGen.Resolution;

internal static class SpriteNameResolver
{
    internal static string? Resolve(ComponentNode? image, AssetNameResolver resolver)
    {
        if (image is null)
            return null;

        var spriteRef = image.GetReference("m_Sprite");
        return resolver.ResolveName(spriteRef?.Guid);
    }
}
