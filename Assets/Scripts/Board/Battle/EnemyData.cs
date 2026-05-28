using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Настройки конкретного монстра: его имя, картинка, уровень, возможные усиления, награды и штрафы
/// </summary>

[CreateAssetMenu(fileName = "EnemyData", menuName = "Board Game/Enemy Data")]
public sealed class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyId;
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemySprite;
    [SerializeField, Min(1)] private int baseLevel = 1;
    [SerializeField, Min(0)] private int bonusPower;
    [SerializeField] private List<EnemyModifier> modifiers = new List<EnemyModifier>();
    [SerializeField] private List<EffectData> penaltyEffects = new List<EffectData>();

    /// <summary>
    /// Id монстра для сохранений и восстановления боя
    /// </summary>
    public string EnemyId => enemyId;
    /// <summary>
    /// Имя монстра для окна боя и логов
    /// </summary>
    public string EnemyName => enemyName;
    /// <summary>
    /// Картинка монстра для окна боя
    /// </summary>
    public Sprite EnemySprite => enemySprite;
    /// <summary>
    /// Базовый уровень монстра без случайных модификаторов
    /// </summary>
    public int BaseLevel => baseLevel;
    /// <summary>
    /// Постоянный бонус силы монстра сверх базового уровня
    /// </summary>
    public int BonusPower => bonusPower;
    /// <summary>
    /// Список усилений, которые могут выпасть этому монстру
    /// </summary>
    public IReadOnlyList<EnemyModifier> Modifiers => modifiers;
    /// <summary>
    /// Список эффектов, которые применяются при штрафе от монстра
    /// </summary>
    public IReadOnlyList<EffectData> PenaltyEffects => penaltyEffects;
    /// <summary>
    /// Исправляет уровень монстра и настройки баланса после правок в инспекторе
    /// </summary>
    private void OnValidate()
    {
        baseLevel = Mathf.Max(1, baseLevel);
        bonusPower = Mathf.Max(0, bonusPower);
        if (modifiers == null)
            modifiers = new List<EnemyModifier>();
        if (penaltyEffects == null)
            penaltyEffects = new List<EffectData>();
    }
}
