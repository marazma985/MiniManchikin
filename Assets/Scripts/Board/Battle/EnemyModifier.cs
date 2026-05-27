using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Настройка возможного усиления монстра, которое может выпасть при открытии боя
/// </summary>

[Serializable]
public sealed class EnemyModifier
{
    [SerializeField] private string modifierName;
    [SerializeField] private List<EffectData> effects = new List<EffectData>();

    /// <summary>
    /// Название усиления монстра для окна боя и логов
    /// </summary>
    public string ModifierName => modifierName;
    /// <summary>
    /// Эффекты, которые усиление добавляет монстру
    /// </summary>
    public IReadOnlyList<EffectData> Effects => effects;
}
