namespace EmberConfig.UI;

using System;
using EmberConfig.Core;

internal static class OptionResolver
{
    internal static object?[] Resolve(ISettingEntry entry)
    {
        var type = entry.Config.SettingType;
        var acceptable = entry.Config.Description?.AcceptableValues;

        if (acceptable is not null)
        {
            var values = AcceptableValueResolver.TryGetList(acceptable);
            if (values is not null)
                return values;
        }

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            var result = new object?[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = values.GetValue(i);
            return result;
        }

        return new[] { entry.Config.BoxedValue };
    }

    internal static string GetDisplayName(object? value) =>
        value?.ToString() ?? string.Empty;

    internal static int FindIndexByDisplayName(object?[] options, object current)
    {
        string name = GetDisplayName(current);
        for (int i = 0; i < options.Length; i++)
        {
            if (GetDisplayName(options[i]) == name)
                return i;
        }
        return -1;
    }
}
