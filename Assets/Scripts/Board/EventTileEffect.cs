using UnityEngine;
using System.Collections.Generic;

public sealed class EventTileEffect : ITileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;
    private IReadOnlyList<EffectData> debuffEvents;

    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
        debuffEvents = newDebuffEvents;
    }

    public void Resolve(BoardTile tile)
    {
        var effect = GetRandomEffect();
        if (effect == null)
        {
            Debug.LogWarning("Random event tile has no events to resolve.");
            return;
        }

        effectResolver.TryApply(effect, "Random event tile");
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null)
            return null;

        var hasBuffEvents = buffEvents != null && buffEvents.Count > 0;
        var hasDebuffEvents = debuffEvents != null && debuffEvents.Count > 0;

        if (!hasBuffEvents && !hasDebuffEvents)
            return null;

        if (hasBuffEvents && hasDebuffEvents)
            return Random.Range(0, 2) == 0 ? GetRandomBuffEffect() : GetRandomDebuffEffect();

        return hasBuffEvents ? GetRandomBuffEffect() : GetRandomDebuffEffect();
    }

    private EffectData GetRandomBuffEffect()
    {
        return buffEvents[Random.Range(0, buffEvents.Count)];
    }

    private EffectData GetRandomDebuffEffect()
    {
        return debuffEvents[Random.Range(0, debuffEvents.Count)];
    }
}
