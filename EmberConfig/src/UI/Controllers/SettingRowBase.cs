namespace EmberConfig.UI;

using System;
using EmberConfig.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal abstract class SettingRowBase : ISettingRow
{
    public Transform Transform { get; }
    public GameObject GameObject => Transform.gameObject;

    protected SettingRowBase(Transform transform)
    {
        Transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }

    public ISettingEntry? Entry { get; private set; }

    private Action? valueChanged;

    public void Bind(ISettingEntry entry)
    {
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        valueChanged = OnRefresh;
        entry.ValueChanged += valueChanged;
        OnBind(entry);
        OnRefresh();
    }

    public void Refresh() => OnRefresh();

    public void Unbind()
    {
        if (Entry is not null && valueChanged is not null)
            Entry.ValueChanged -= valueChanged;
        Entry = null;
        valueChanged = null;
        OnUnbind();
    }

    public virtual void Update() { }
    public virtual void UpdateHover()
    {
        if (!GameObject.activeInHierarchy)
            return;

        var hover = Transform.GetComponent<RowHoverHandler>();
        if (hover is not null && hover.enabled)
            hover.UpdateHover();
    }
    public virtual bool IsHovered => Transform.GetComponent<RowHoverHandler>()?.IsHovered ?? false;
    public virtual string Description => Entry?.Description ?? string.Empty;
    public virtual bool IsCapturing => false;

    protected abstract void OnBind(ISettingEntry entry);
    protected abstract void OnRefresh();
    protected virtual void OnUnbind() { }

    protected void SetValue(object? value, bool save = true)
    {
        if (Entry is null) return;
        Entry.Config.BoxedValue = value;
        if (save && Entry.Config?.ConfigFile is not null)
            Entry.Config.ConfigFile.Save();
    }

    protected static void SafeSetText(TextMeshProUGUI? text, string value)
    {
        if (text is not null)
            text.text = value;
    }

    protected TextMeshProUGUI? FindTitleText()
    {
        var title = Transform.Find("Title");
        if (title is not null)
            return title.GetComponent<TextMeshProUGUI>();

        return Transform.GetComponentInChildren<TextMeshProUGUI>(true);
    }
}
