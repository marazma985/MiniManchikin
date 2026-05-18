using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class RareTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> rareEvents;

    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newRareEvents)
    {
        effectResolver = newEffectResolver;
        rareEvents = newRareEvents;
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
            Debug.LogWarning("Rare event tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Rare event tile", onResolved);
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || rareEvents == null || rareEvents.Count == 0)
            return null;

        return rareEvents[UnityEngine.Random.Range(0, rareEvents.Count)];
    }
}
