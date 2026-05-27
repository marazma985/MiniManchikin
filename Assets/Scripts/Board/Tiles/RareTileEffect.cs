using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Эффект редкой клетки, который выбирает более особое событие
/// </summary>

public sealed class RareTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> rareEvents;
    /// <summary>
    /// Подключает список редких эффектов, которые может выдать rare-клетка
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newRareEvents)
    {
        effectResolver = newEffectResolver;
        rareEvents = newRareEvents;
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Resolve(tile, null);
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
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
    /// <summary>
    /// Выбирает случайный редкий эффект для rare-клетки
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || rareEvents == null || rareEvents.Count == 0)
            return null;

        return rareEvents[UnityEngine.Random.Range(0, rareEvents.Count)];
    }
}
