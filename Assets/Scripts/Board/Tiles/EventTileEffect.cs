using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Эффект обычной событийной клетки, который запускает случайное событие
/// </summary>

public sealed class EventTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;
    private IReadOnlyList<EffectData> debuffEvents;
    /// <summary>
    /// Подключает положительные и отрицательные эффекты для случайной event-клетки
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
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
            Debug.LogWarning("Random event tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Random event tile", onResolved);
    }
    /// <summary>
    /// Выбирает случайный положительный или отрицательный эффект для event-клетки
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null)
            return null;

        var hasBuffEvents = buffEvents != null && buffEvents.Count > 0;
        var hasDebuffEvents = debuffEvents != null && debuffEvents.Count > 0;

        if (!hasBuffEvents && !hasDebuffEvents)
            return null;

        if (hasBuffEvents && hasDebuffEvents)
            return UnityEngine.Random.Range(0, 2) == 0 ? GetRandomBuffEffect() : GetRandomDebuffEffect();

        return hasBuffEvents ? GetRandomBuffEffect() : GetRandomDebuffEffect();
    }
    /// <summary>
    /// Выбирает случайный положительный эффект для event-клетки
    /// </summary>
    private EffectData GetRandomBuffEffect()
    {
        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }
    /// <summary>
    /// Выбирает случайный отрицательный эффект для event-клетки
    /// </summary>
    private EffectData GetRandomDebuffEffect()
    {
        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
