namespace SettingsLib.UI;

using System;
using System.Reflection;
using BepInEx.Configuration;

/// <summary>
/// Resolves min/max values and allowed lists from BepInEx <see cref="AcceptableValueBase"/> instances
/// without duplicating reflection logic across UI rows.
/// </summary>
internal static class AcceptableValueResolver
{
    private static readonly Type RangeTypeDefinition = typeof(AcceptableValueRange<>);
    private static readonly Type ListTypeDefinition = typeof(AcceptableValueList<>);
    private static readonly MethodInfo TryGetRangeCoreMethod = typeof(AcceptableValueResolver).GetMethod(nameof(TryGetRangeCore), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo TryGetListCoreMethod = typeof(AcceptableValueResolver).GetMethod(nameof(TryGetListCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Attempts to extract the numeric minimum and maximum from an <see cref="AcceptableValueRange{T}"/>.
    /// </summary>
    /// <param name="range">The acceptable value range, or <c>null</c>.</param>
    /// <param name="min">The minimum value, or <c>0</c> when not a range.</param>
    /// <param name="max">The maximum value, or <c>0</c> when not a range.</param>
    /// <returns><c>true</c> when the instance is a supported range and the values could be converted to floats.</returns>
    public static bool TryGetRange(AcceptableValueBase? range, out float min, out float max)
    {
        min = 0f;
        max = 0f;

        if (range is null)
            return false;

        var type = range.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != RangeTypeDefinition)
            return false;

        var valueType = type.GetGenericArguments()[0];
        var core = TryGetRangeCoreMethod.MakeGenericMethod(valueType);

        var result = core.Invoke(null, new[] { range });
        if (result is not ValueTuple<bool, float, float> tuple || !tuple.Item1)
            return false;

        min = tuple.Item2;
        max = tuple.Item3;
        return true;
    }

    /// <summary>
    /// Attempts to extract the allowed values from an <see cref="AcceptableValueList{T}"/>.
    /// </summary>
    /// <param name="list">The acceptable value list, or <c>null</c>.</param>
    /// <returns>The allowed values, or <c>null</c> when the instance is not a supported list.</returns>
    public static object?[]? TryGetList(AcceptableValueBase? list)
    {
        if (list is null)
            return null;

        var type = list.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != ListTypeDefinition)
            return null;

        var valueType = type.GetGenericArguments()[0];
        var core = TryGetListCoreMethod.MakeGenericMethod(valueType);

        return (object?[]?)core.Invoke(null, new[] { list });
    }

    private static (bool Success, float Min, float Max) TryGetRangeCore<T>(AcceptableValueRange<T> range)
        where T : IComparable
    {
        var min = Convert.ToSingle(range.MinValue);
        var max = Convert.ToSingle(range.MaxValue);
        return (true, min, max);
    }

    private static object?[]? TryGetListCore<T>(AcceptableValueList<T> list)
        where T : IEquatable<T>
    {
        var values = list.AcceptableValues;
        if (values is null)
            return null;

        var result = new object?[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = values[i];

        return result;
    }
}
