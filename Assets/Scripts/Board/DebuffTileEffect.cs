using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class DebuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> debuffEvents;

    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
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
            Debug.LogWarning("Debuff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Debuff tile", onResolved);
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || debuffEvents == null || debuffEvents.Count == 0)
            return null;

        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
