using UnityEngine;
using System.Collections.Generic;

public sealed class DebuffTileEffect : ITileEffect
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
        var effect = GetRandomEffect();
        if (effect == null)
        {
            Debug.LogWarning("Debuff tile has no events to resolve.");
            return;
        }

        effectResolver.TryApply(effect, "Debuff tile");
    }

    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || debuffEvents == null || debuffEvents.Count == 0)
            return null;

        return debuffEvents[Random.Range(0, debuffEvents.Count)];
    }
}
