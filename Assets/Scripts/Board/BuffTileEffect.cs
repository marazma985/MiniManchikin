using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class BuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;

    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
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
            Debug.LogWarning("Buff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Buff tile", onResolved);
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || buffEvents == null || buffEvents.Count == 0)
            return null;

        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }
}
