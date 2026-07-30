namespace EmberConfig.UI;

using System;
using BepInEx.Configuration;
using EmberConfig;
using EmberConfig.Core;
using EmberConfig.Public;
using UnityEngine;

internal static class RowTypeResolver
{
    internal static RowType Resolve(ISettingEntry entry)
    {
        if (entry is KeybindEntry)
            return RowType.Keybind;

        var controlStyle = SanitizeControlStyle(entry);
        if (controlStyle == SettingControlStyle.Switch)
            return RowType.Switch;

        if (controlStyle == SettingControlStyle.Dropdown)
            return RowType.Dropdown;

        if (controlStyle == SettingControlStyle.Carousel)
            return RowType.Carousel;

        var type = entry.Config.SettingType;
        if (type == typeof(bool))
            return RowType.Switch;

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

    private static SettingControlStyle SanitizeControlStyle(ISettingEntry entry)
    {
        var controlStyle = entry.ControlStyle;
        if (controlStyle == SettingControlStyle.Auto)
            return controlStyle;

        var type = entry.Config.SettingType;
        var acceptable = entry.Config.Description?.AcceptableValues;
        var isCompatible = controlStyle switch
        {
            SettingControlStyle.Switch => type == typeof(bool),
            SettingControlStyle.Dropdown or SettingControlStyle.Carousel => type.IsEnum || IsAcceptableValueList(acceptable),
            _ => true,
        };

        if (isCompatible)
            return controlStyle;

        Plugin.Logger?.LogWarning($"EmberConfig: ControlStyle {controlStyle} is not valid for type {type} on '{entry.Label}'; using Auto.");
        return SettingControlStyle.Auto;
    }

    private static bool IsAcceptableValueList(AcceptableValueBase? acceptable)
    {
        if (acceptable is null)
            return false;

        var type = acceptable.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AcceptableValueList<>);
    }
}
