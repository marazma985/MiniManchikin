using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с DebuffTileEffect
/// </summary>

public sealed class DebuffTileEffect : IDeferredTileEffect
{
    private EffectResolver effectResolver;
    private IReadOnlyList<EffectData> debuffEvents;
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(EffectResolver newEffectResolver, IReadOnlyList<EffectData> newDebuffEvents)
    {
        effectResolver = newEffectResolver;
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
            Debug.LogWarning("Debuff tile has no events to resolve.");
            onResolved?.Invoke();
            return;
        }

        effectResolver.TryApply(effect, "Debuff tile", onResolved);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectData GetRandomEffect()
    {
        if (effectResolver == null || debuffEvents == null || debuffEvents.Count == 0)
            return null;

        return debuffEvents[UnityEngine.Random.Range(0, debuffEvents.Count)];
    }
}
