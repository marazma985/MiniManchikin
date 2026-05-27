using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с BuffTileEffect
/// </summary>

public sealed class BuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> buffEvents;
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newBuffEvents)
    {
        effectResolver = newEffectResolver;
        buffEvents = newBuffEvents;
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
            Debug.LogWarning("Buff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Buff tile", onResolved);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || buffEvents == null || buffEvents.Count == 0)
            return null;

        return buffEvents[UnityEngine.Random.Range(0, buffEvents.Count)];
    }
}
