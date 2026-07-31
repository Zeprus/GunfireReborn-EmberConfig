namespace EmberConfig.PrefabDataGen.Parsing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EmberConfig.PrefabDataGen.Models;
using YamlDotNet.RepresentationModel;

internal sealed class ComponentNode
{
    public long FileID { get; }
    public int UnityType { get; }
    public string TypeName { get; }
    public YamlMappingNode Properties { get; }

    public ComponentNode(long fileID, int unityType, string typeName, YamlMappingNode properties)
    {
        FileID = fileID;
        UnityType = unityType;
        TypeName = typeName;
        Properties = properties;
    }

    public string? GetString(string key) =>
        Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar
            ? scalar.Value
            : null;

    public int? GetInt(string key) =>
        Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar &&
        int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    public float? GetFloat(string key) =>
        Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar &&
        float.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    public uint? GetUInt(string key) =>
        Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar &&
        uint.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    public bool? GetBool(string key)
    {
        if (Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar)
        {
            var value = scalar.Value?.ToLowerInvariant() ?? string.Empty;
            if (value is "1" or "true")
                return true;
            if (value is "0" or "false")
                return false;
        }

        return null;
    }

    public T? GetEnum<T>(string key) where T : struct, Enum
    {
        if (Properties.TryGetChild(key, out var node) && node is YamlScalarNode scalar &&
            int.TryParse(scalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        return null;
    }

    public YamlMappingNode? GetMapping(string key) =>
        Properties.TryGetChild(key, out var node) ? node as YamlMappingNode : null;

    public YamlSequenceNode? GetSequence(string key) =>
        Properties.TryGetChild(key, out var node) ? node as YamlSequenceNode : null;

    public FileIdReference? GetReference(string key)
    {
        var mapping = GetMapping(key);
        if (mapping is null)
            return null;

        if (!mapping.TryGetChild("fileID", out var fileIdNode) || fileIdNode is not YamlScalarNode fileIdScalar)
            return null;

        long fileId = long.Parse(fileIdScalar.Value!, CultureInfo.InvariantCulture);
        string? guid = null;

        if (mapping.TryGetChild("guid", out var guidNode) && guidNode is YamlScalarNode guidScalar)
            guid = guidScalar.Value;

        return new FileIdReference(fileId, guid);
    }

    public Color? GetColor(string key)
    {
        var mapping = GetMapping(key);
        if (mapping is null)
            return null;

        return YamlParsers.ParseColor(mapping);
    }

    public Vector2? GetVector2(string key)
    {
        var mapping = GetMapping(key);
        if (mapping is null)
            return null;

        return YamlParsers.ParseVector2(mapping);
    }

    public Vector3? GetVector3(string key)
    {
        var mapping = GetMapping(key);
        if (mapping is null)
            return null;

        return YamlParsers.ParseVector3(mapping);
    }

    public IReadOnlyList<long> GetFileIDList(string key)
    {
        var sequence = GetSequence(key);
        if (sequence is null)
            return Array.Empty<long>();

        var list = new List<long>();
        foreach (var child in sequence.Children)
        {
            if (child is YamlMappingNode mapping &&
                mapping.TryGetChild("fileID", out var fileIdNode) &&
                fileIdNode is YamlScalarNode fileIdScalar &&
                long.TryParse(fileIdScalar.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fileId))
            {
                list.Add(fileId);
            }
        }

        return list;
    }

    public string? GetScriptGuid()
    {
        var reference = GetReference("m_Script");
        return reference?.Guid;
    }
}

internal readonly record struct FileIdReference(long FileID, string? Guid);
