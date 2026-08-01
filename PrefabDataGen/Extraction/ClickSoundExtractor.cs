namespace EmberConfig.PrefabDataGen.Extraction;

using System.Linq;
using EmberConfig.PrefabDataGen.Parsing;

internal static class ClickSoundExtractor
{
    internal static uint? Extract(GameObjectNode? button)
    {
        if (button is null)
            return null;

        var trigger = button.Components.FirstOrDefault(ComponentPredicates.IsAkTriggerMouseUp);
        var eventId = trigger?.GetInt("eventIdInternal");
        if (eventId is null)
            return null;

        return unchecked((uint)eventId.Value);
    }
}
