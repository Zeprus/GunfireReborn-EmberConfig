namespace EmberConfig.PrefabDataGen.Models;

internal readonly record struct Color(float R, float G, float B, float A)
{
    internal static Color Multiply(Color color, float multiplier) =>
        new(color.R * multiplier, color.G * multiplier, color.B * multiplier, color.A * multiplier);
}
