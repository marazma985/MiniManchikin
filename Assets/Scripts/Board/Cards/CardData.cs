using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Настройки карты: название, описание, картинка, редкость и игровые эффекты
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

    /// <summary>
    /// Id карты для сохранений и восстановления руки
    /// </summary>
    public string CardId => cardId;
    /// <summary>
    /// Название карты, которое видит игрок
    /// </summary>
    public string CardName => cardName;
    /// <summary>
    /// Описание карты для интерфейса
    /// </summary>
    public string Description => description;
    /// <summary>
    /// Картинка карты для руки и наград
    /// </summary>
    public Sprite CardSprite => cardSprite;
    /// <summary>
    /// Редкость карты, по которой игра выбирает награды и случайное удаление
    /// </summary>
    public Rarity Rarity => rarity;
    /// <summary>
    /// Место, где карту разрешено использовать
    /// </summary>
    public UsageContext UsageContext => usageContext;
    /// <summary>
    /// Эффекты, которые применяются после использования карты
    /// </summary>
    public IReadOnlyList<EffectData> Effects => effects;
    /// <summary>
    /// Автоматически подставляет id карты по имени ассета, если id пустой
    /// </summary>
    private void OnValidate()
    {
        if (effects == null)
            effects = new List<EffectData>();
    }
}
