using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Board Game/Enemy Data")]
public sealed class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyId;
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemySprite;
    [SerializeField, FormerlySerializedAs("level"), Min(1)] private int baseLevel = 1;
    [SerializeField, Min(0)] private int bonusPower;
    [SerializeField] private MonsterPenaltyType penaltyType;
    [SerializeField, Min(0)] private int penaltyValue = 1;
    [SerializeField] private List<EnemyModifier> modifiers = new List<EnemyModifier>();
    [SerializeField] private List<EffectData> penaltyEffects = new List<EffectData>();

    public string EnemyId => enemyId;
    public string EnemyName => enemyName;
    public Sprite EnemySprite => enemySprite;
    public int BaseLevel => baseLevel;
    public int Level => baseLevel;
    public int BonusPower => bonusPower;
    public MonsterPenaltyType PenaltyType => penaltyType;
    public int PenaltyValue => penaltyValue;
    public IReadOnlyList<EnemyModifier> Modifiers => modifiers;
    public IReadOnlyList<EffectData> PenaltyEffects => penaltyEffects;

    private void OnValidate()
    {
        baseLevel = Mathf.Max(1, baseLevel);
        bonusPower = Mathf.Max(0, bonusPower);
        penaltyValue = Mathf.Max(0, penaltyValue);
        if (modifiers == null)
            modifiers = new List<EnemyModifier>();
        if (penaltyEffects == null)
            penaltyEffects = new List<EffectData>();
    }
}
