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

    public string ModifierName => modifierName;
    public IReadOnlyList<EffectData> Effects => effects;
}
