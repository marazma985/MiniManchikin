using UnityEngine;
using System.Collections.Generic;

public sealed class BuffTileEffect : ITileEffect
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
        var effect = GetRandomEffect();
        if (effect == null)
        {
            Debug.LogWarning("Buff tile has no events to resolve.");
            return;
        }

        effectResolver.TryApply(effect, "Buff tile");
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || buffEvents == null || buffEvents.Count == 0)
            return null;

        return buffEvents[Random.Range(0, buffEvents.Count)];
    }
}
