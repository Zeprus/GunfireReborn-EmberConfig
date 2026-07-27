namespace SettingsLib.UI;

using UnityEngine;

internal static class WwiseAudio
{
    internal static void PostIfValid(uint eventId, GameObject? gameObject)
    {
        if (eventId != 0u && gameObject is not null)
            AkSoundEngine.PostEvent(eventId, gameObject);
    }

    internal static uint GetEventId(Transform? target)
    {
        if (target is null)
            return 0u;

        var akEvent = target.GetComponent<AkEvent>() ?? target.GetComponentInChildren<AkEvent>(true);
        if (akEvent is not null)
            return akEvent.data?.Id ?? 0u;

        for (var t = target.parent; t is not null; t = t.parent)
        {
            akEvent = t.GetComponent<AkEvent>();
            if (akEvent is not null)
                return akEvent.data?.Id ?? 0u;
        }

        return 0u;
    }
}
