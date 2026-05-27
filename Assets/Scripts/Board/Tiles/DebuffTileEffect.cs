using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Эффект отрицательной клетки, который выбирает неприятное событие для игрока
/// </summary>

public sealed class DebuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> debuffEvents;
    /// <summary>
    /// Подключает список отрицательных эффектов, которые может выдать дебафф-клетка
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
        debuffEvents = newDebuffEvents;
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
            Debug.LogWarning("Debuff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Debuff tile", onResolved);
    }
    /// <summary>
    /// Выбирает случайный отрицательный эффект для дебафф-клетки
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || debuffEvents == null || debuffEvents.Count == 0)
            return null;

        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
