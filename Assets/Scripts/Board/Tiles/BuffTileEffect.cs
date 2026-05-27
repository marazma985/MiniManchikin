using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Эффект положительной клетки, который выбирает полезное событие для игрока
/// </summary>

public sealed class BuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;
    /// <summary>
    /// Подключает список положительных эффектов, которые может выдать бафф-клетка
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
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
            Debug.LogWarning("Buff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Buff tile", onResolved);
    }
    /// <summary>
    /// Выбирает случайный положительный эффект для бафф-клетки
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || buffEvents == null || buffEvents.Count == 0)
            return null;

        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }
}
