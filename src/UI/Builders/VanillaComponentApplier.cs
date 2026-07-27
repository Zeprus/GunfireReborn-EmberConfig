namespace SettingsLib.UI;

using DYControl;
using UnityEngine;
using UnityEngine.UI;

internal static class VanillaComponentApplier
{
    internal static void ApplyToRow(Transform rowRoot, Selectable control)
    {
        if (rowRoot is null || control is null)
            return;

        var dySelect = rowRoot.GetComponent<DYSelect>() ?? rowRoot.gameObject.AddComponent<DYSelect>();
        dySelect.unitySel = control;
        dySelect.defaultCanSelect = true;
        dySelect.firstSelect = false;
        dySelect.selectExcuteClick = false;
        dySelect.BelongId = 0;
        dySelect.isDown = false;
    }

    internal static void ApplyToControl(Transform controlTransform, bool addDySelect = true, bool addAudio = true)
    {
        if (controlTransform is null)
            return;

        var selectable = controlTransform.GetComponent<Selectable>();
        if (selectable is null)
            return;

        if (addDySelect)
            ApplyToRow(controlTransform, selectable);

        if (addAudio)
            AttachAudio(controlTransform);
    }

    internal static void AttachAudio(Transform target)
    {
        var go = target.gameObject;
        var akGameObj = go.GetComponent<AkGameObj>() ?? go.AddComponent<AkGameObj>();
        akGameObj.isStaticObject = true;
        akGameObj.isEnvironmentAware = false;

        if (target.GetComponent<M1Button>() is M1Button m1Button)
            akGameObj.btn = m1Button;

        if (go.GetComponent<AkTriggerMouseClick>() is null)
            go.AddComponent<AkTriggerMouseClick>();
    }
}
