using System.Collections.Generic;
/// <summary>
/// Каталог ассетов, через который сохранение находит карты, предметы и монстров по их id
/// </summary>

public sealed class GameSaveContentResolver
{
    private readonly Dictionary<string, CardData> cardsById = new Dictionary<string, CardData>();
    private readonly Dictionary<string, ItemData> itemsById = new Dictionary<string, ItemData>();
    private readonly Dictionary<string, EnemyData> enemiesById = new Dictionary<string, EnemyData>();
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    public void AddCard(CardData card)
    {
        if (card == null || string.IsNullOrEmpty(card.CardId))
            return;

        cardsById[card.CardId] = card;
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    public void AddItem(ItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.ItemId))
            return;

        itemsById[item.ItemId] = item;
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    public void AddEnemy(EnemyData enemy)
    {
        if (enemy == null || string.IsNullOrEmpty(enemy.EnemyId))
            return;

        enemiesById[enemy.EnemyId] = enemy;
    }
    /// <summary>
    /// Находит карту из сохранения по ее id
    /// </summary>
    public CardData GetCard(string cardId)
    {
        return !string.IsNullOrEmpty(cardId) && cardsById.TryGetValue(cardId, out var card) ? card : null;
    }
    /// <summary>
    /// Находит предмет из сохранения по его id
    /// </summary>
    public ItemData GetItem(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && itemsById.TryGetValue(itemId, out var item) ? item : null;
    }
    /// <summary>
    /// Находит монстра из сохранения по его id
    /// </summary>
    public EnemyData GetEnemy(string enemyId)
    {
        return !string.IsNullOrEmpty(enemyId) && enemiesById.TryGetValue(enemyId, out var enemy) ? enemy : null;
    }
    /// <summary>
    /// Восстанавливает награду из сохраненных данных
    /// </summary>
    public RewardData GetReward(RewardSaveData saveData)
    {
        if (saveData == null)
            return null;

        switch ((RewardType)saveData.rewardType)
        {
            case RewardType.Card:
                return RewardData.FromCard(GetCard(saveData.contentId));
            case RewardType.Item:
                return RewardData.FromItem(GetItem(saveData.contentId));
            default:
                return null;
        }
    }
}
