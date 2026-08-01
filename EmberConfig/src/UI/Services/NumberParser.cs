namespace EmberConfig.UI;

using System;
using System.Globalization;

/// <summary>
/// Parses numeric input in a culture-tolerant way.
/// First tries the invariant (english) format, then the current culture format,
/// so both "0.5" and "0,5" work depending on the player's locale.
/// </summary>
internal static class NumberParser
{
    /// <summary>
    /// Tries to parse a <see cref="float"/> from <paramref name="text"/> using invariant
    /// then current culture.
    /// </summary>
    public static bool TryParseFloat(string text, out float value)
    {
        if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        return float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    /// <summary>
    /// Tries to parse an <see cref="int"/> from <paramref name="text"/> using invariant
    /// then current culture.
    /// </summary>
    public static bool TryParseInt(string text, out int value)
    {
        if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        return int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    /// <summary>
    /// Converts <paramref name="text"/> to <paramref name="targetType"/> using invariant
    /// then current culture. Returns <c>null</c> for <see cref="string"/> inputs.
    /// </summary>
    public static object? ConvertToType(string text, Type targetType)
    {
        if (targetType == typeof(string))
            return text;

        try
        {
            return Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return Convert.ChangeType(text, targetType, CultureInfo.CurrentCulture);
        }
    }
}
