namespace EmberConfig.UI;

using EmberConfig.Core;
using UnityEngine;

internal interface ISettingRow
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
