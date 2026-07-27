namespace SettingsLib.UI;

using SettingsLib.Core;
using UnityEngine;

public interface ISettingRow
{
    Transform Transform { get; }
    GameObject GameObject { get; }

    void Bind(ISettingEntry entry);
    void Refresh();
    void Unbind();
    void Update();
    void UpdateHover();

    bool IsHovered { get; }
    string Description { get; }
    bool IsCapturing { get; }
}
