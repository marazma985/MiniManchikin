using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с RareTileEffect
/// </summary>

public sealed class RareTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> rareEvents;
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newRareEvents)
    {
        effectResolver = newEffectResolver;
        rareEvents = newRareEvents;
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
            Debug.LogWarning("Rare event tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Rare event tile", onResolved);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || rareEvents == null || rareEvents.Count == 0)
            return null;

        return rareEvents[UnityEngine.Random.Range(0, rareEvents.Count)];
    }
}
