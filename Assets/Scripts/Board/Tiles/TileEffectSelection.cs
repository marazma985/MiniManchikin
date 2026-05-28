using System;
using System.Collections.Generic;
using UnityEngine;

internal static class TileEffectSelection
{
    public static EffectData PickRandom(IReadOnlyList<EffectData> effects)
    {
        if (effects == null || effects.Count == 0)
            return null;

        return effects[UnityEngine.Random.Range(0, effects.Count)];
    }

    public static void ApplyOrComplete(TileResolutionContext context, EffectData effect, string sourceName, Action onResolved)
    {
        if (effect == null)
        {
            Debug.LogWarning($"{sourceName} has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        if (context == null || context.EffectResolver == null)
        {
            Debug.LogWarning($"{sourceName} requires an effect resolver.");
            onResolved?.Invoke();
            return;
        }

        context.EffectResolver.TryApply(effect, sourceName, onResolved);
    }
}
