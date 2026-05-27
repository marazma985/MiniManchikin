using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Отвечает за работу карт и логику, связанную с CardData
/// </summary>

[CreateAssetMenu(fileName = "CardData", menuName = "Board Game/Card Data")]
public sealed class CardData : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private Rarity rarity;
    [SerializeField] private UsageContext usageContext = UsageContext.Anywhere;
    [SerializeField] private List<EffectData> effects = new List<EffectData>();

    public string CardId => cardId;
    public string CardName => cardName;
    public string Description => description;
    public Sprite CardSprite => cardSprite;
    public Rarity Rarity => rarity;
    public UsageContext UsageContext => usageContext;
    public IReadOnlyList<EffectData> Effects => effects;
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (effects == null)
            effects = new List<EffectData>();
    }
}
