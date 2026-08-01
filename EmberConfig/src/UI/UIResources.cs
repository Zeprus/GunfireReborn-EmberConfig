namespace EmberConfig.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared runtime UI assets that are not available from the dumped prefabs.
/// </summary>
internal static class UIResources
{
    private static Sprite? whiteSprite;

    /// <summary>
    /// A 1x1 white <see cref="Sprite"/> with <see cref="Image.Type.Simple"/>.
    /// Used as the hit target for otherwise-invisible <see cref="Image"/> components
    /// (tab bar viewport, tab buttons, etc.) because an <see cref="Image"/> without
    /// a sprite has no geometry and cannot be raycast.
    /// </summary>
    internal static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite is null)
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();

                whiteSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 2f, 2f),
                    new Vector2(0.5f, 0.5f),
                    1f,
                    0u,
                    SpriteMeshType.FullRect);

                if (whiteSprite is not null)
                    whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            }

            return whiteSprite!;
        }
    }
}
