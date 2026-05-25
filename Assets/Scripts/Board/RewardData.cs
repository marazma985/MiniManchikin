using System;
using UnityEngine;

[Serializable]
public sealed class RewardData
{
    [SerializeField] private RewardType rewardType;
    [SerializeField] private CardData cardData;
    [SerializeField] private ItemData itemData;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite displaySprite;

    public RewardType RewardType => rewardType;
    public CardData CardData => cardData;
    public ItemData ItemData => itemData;
    public string DisplayName => displayName;
    public Sprite DisplaySprite => displaySprite;
    public string DisplayDescription
    {
        get
        {
            switch (rewardType)
            {
                case RewardType.Card:
                    return cardData != null ? cardData.Description : string.Empty;
                case RewardType.Item:
                    return itemData != null ? itemData.Description : string.Empty;
                default:
                    return string.Empty;
            }
        }
    }

    private RewardData(RewardType rewardType, CardData cardData, ItemData itemData, string displayName, Sprite displaySprite)
    {
        this.rewardType = rewardType;
        this.cardData = cardData;
        this.itemData = itemData;
        this.displayName = displayName;
        this.displaySprite = displaySprite;
    }

    public static RewardData FromCard(CardData card)
    {
        if (card == null)
            return null;

        return new RewardData(RewardType.Card, card, null, card.CardName, card.CardSprite);
    }

    public static RewardData FromItem(ItemData item)
    {
        if (item == null)
            return null;

        return new RewardData(RewardType.Item, null, item, item.ItemName, item.ItemSprite);
    }
}
