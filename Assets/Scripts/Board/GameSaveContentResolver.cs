using System.Collections.Generic;

public sealed class GameSaveContentResolver
{
    private readonly Dictionary<string, CardData> cardsById = new Dictionary<string, CardData>();
    private readonly Dictionary<string, ItemData> itemsById = new Dictionary<string, ItemData>();
    private readonly Dictionary<string, EnemyData> enemiesById = new Dictionary<string, EnemyData>();

    public void AddCard(CardData card)
    {
        if (card == null || string.IsNullOrEmpty(card.CardId))
            return;

        cardsById[card.CardId] = card;
    }

    public void AddItem(ItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.ItemId))
            return;

        itemsById[item.ItemId] = item;
    }

    public void AddEnemy(EnemyData enemy)
    {
        if (enemy == null || string.IsNullOrEmpty(enemy.EnemyId))
            return;

        enemiesById[enemy.EnemyId] = enemy;
    }

    public CardData GetCard(string cardId)
    {
        return !string.IsNullOrEmpty(cardId) && cardsById.TryGetValue(cardId, out var card) ? card : null;
    }

    public ItemData GetItem(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && itemsById.TryGetValue(itemId, out var item) ? item : null;
    }

    public EnemyData GetEnemy(string enemyId)
    {
        return !string.IsNullOrEmpty(enemyId) && enemiesById.TryGetValue(enemyId, out var enemy) ? enemy : null;
    }

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
