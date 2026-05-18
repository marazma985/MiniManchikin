using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class EventTileEffect : IDeferredTileEffect
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
        Resolve(tile, null);
    }

    public void Resolve(BoardTile tile, Action onResolved)
    {
        var effect = GetRandomEffect();
        if (effect == null)
        {
            Debug.LogWarning("Random event tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Random event tile", onResolved);
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
            return UnityEngine.Random.Range(0, 2) == 0 ? GetRandomBuffEffect() : GetRandomDebuffEffect();

        return hasBuffEvents ? GetRandomBuffEffect() : GetRandomDebuffEffect();
    }

    private EffectData GetRandomBuffEffect()
    {
        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }

    private EffectData GetRandomDebuffEffect()
    {
        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
