using UnityEngine;
using System.Collections.Generic;

public sealed class RareTileEffect : ITileEffect
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
        var effect = GetRandomEffect();
        if (effect == null)
        {
            Debug.LogWarning("Rare event tile has no events to resolve.");
            return;
        }

        effectResolver.TryApply(effect, "Rare event tile");
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || rareEvents == null || rareEvents.Count == 0)
            return null;

        return rareEvents[Random.Range(0, rareEvents.Count)];
    }
}
