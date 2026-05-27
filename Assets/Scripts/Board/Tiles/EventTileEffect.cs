using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с EventTileEffect
/// </summary>

public sealed class EventTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;
    private IReadOnlyList<EffectData> debuffEvents;
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
        debuffEvents = newDebuffEvents;
    }
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Resolve(tile, null);
    }
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
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
    /// Возвращает сохраненное или рассчитанное значение
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
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectData GetRandomBuffEffect()
    {
        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectData GetRandomDebuffEffect()
    {
        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
