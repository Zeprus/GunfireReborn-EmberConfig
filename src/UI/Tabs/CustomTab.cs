namespace SettingsLib.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class CustomTab
{
    public Transform Button { get; }
    public Transform Content { get; }
    public M1Toggle Toggle { get; }

    public CustomTab(Transform button, Transform content, M1Toggle toggle)
    {
        Button = button ?? throw new ArgumentNullException(nameof(button));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Toggle = toggle ?? throw new ArgumentNullException(nameof(toggle));
    }
}
