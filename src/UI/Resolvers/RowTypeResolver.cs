namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig.Core;
using UnityEngine;

internal static class RowTypeResolver
{
    internal static RowType Resolve(ISettingEntry entry)
    {
        if (entry is KeybindEntry)
            return RowType.Keybind;

        var type = entry.Config.SettingType;
        if (type == typeof(bool))
            return RowType.Toggle;

        var acceptable = entry.Config.Description?.AcceptableValues;
        if (acceptable is not null)
        {
            var acceptableType = acceptable.GetType();
            if (acceptableType.IsGenericType)
            {
                var genericDef = acceptableType.GetGenericTypeDefinition();
                if (genericDef == typeof(AcceptableValueList<>))
                    return RowType.Dropdown;
                if (genericDef == typeof(AcceptableValueRange<>))
                    return RowType.Slider;
            }
        }

        if (type.IsEnum)
            return RowType.Dropdown;

        if (type == typeof(string))
            return RowType.InputField;

        if (type == typeof(float) || type == typeof(int))
            return acceptable is not null ? RowType.Slider : RowType.InputField;

        return RowType.InputField;
    }
}
