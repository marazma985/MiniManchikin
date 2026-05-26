using System;
using UnityEngine;

[Serializable]
public abstract class RewardData
{
    public abstract RewardType RewardType { get; }
    public abstract string DisplayName { get; }
    public abstract Sprite DisplaySprite { get; }
    public abstract string DisplayDescription { get; }
    public abstract EffectType ClaimEffectType { get; }

    public virtual CardData CardData => null;
    public virtual ItemData ItemData => null;

    public abstract bool CanClaim(CardSystem cardSystem, PlayerInventory playerInventory, out string status);
    public abstract bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory);

    public static RewardData FromCard(CardData card)
    {
        return card != null ? new CardRewardData(card) : null;
    }

    public static RewardData FromItem(ItemData item)
    {
        return item != null ? new ItemRewardData(item) : null;
    }
}

[Serializable]
public sealed class CardRewardData : RewardData
{
    private readonly CardData cardData;

    public CardRewardData(CardData cardData)
    {
        this.cardData = cardData;
    }

    public override RewardType RewardType => RewardType.Card;
    public override CardData CardData => cardData;
    public override string DisplayName => cardData != null ? cardData.CardName : string.Empty;
    public override Sprite DisplaySprite => cardData != null ? cardData.CardSprite : null;
    public override string DisplayDescription => cardData != null ? cardData.Description : string.Empty;
    public override EffectType ClaimEffectType => EffectType.GiveCard;

    public override bool CanClaim(CardSystem cardSystem, PlayerInventory playerInventory, out string status)
    {
        if (cardSystem == null || cardData == null)
        {
            status = "Система карт не назначена";
            return false;
        }

        if (cardSystem.Hand.Count >= cardSystem.MaxCards)
        {
            status = "Рука карт заполнена";
            return false;
        }

        status = string.Empty;
        return true;
    }

    public override bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory)
    {
        return cardSystem != null && cardData != null && cardSystem.AddCard(cardData);
    }
}

[Serializable]
public sealed class ItemRewardData : RewardData
{
    private readonly ItemData itemData;

    public ItemRewardData(ItemData itemData)
    {
        this.itemData = itemData;
    }

    public override RewardType RewardType => RewardType.Item;
    public override ItemData ItemData => itemData;
    public override string DisplayName => itemData != null ? itemData.ItemName : string.Empty;
    public override Sprite DisplaySprite => itemData != null ? itemData.ItemSprite : null;
    public override string DisplayDescription => itemData != null ? itemData.Description : string.Empty;
    public override EffectType ClaimEffectType => EffectType.GiveItem;

    public override bool CanClaim(CardSystem cardSystem, PlayerInventory playerInventory, out string status)
    {
        if (playerInventory == null || itemData == null)
        {
            status = "Инвентарь экипировки не назначен";
            return false;
        }

        if (!playerInventory.HasFreeSlot())
        {
            status = "Инвентарь экипировки заполнен";
            return false;
        }

        status = string.Empty;
        return true;
    }

    public override bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory)
    {
        return playerInventory != null && itemData != null && playerInventory.TryEquip(itemData);
    }
}
