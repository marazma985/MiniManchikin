using System;
using UnityEngine;
/// <summary>
/// Общая обертка для награды, чтобы карта и предмет выбирались одинаково
/// </summary>

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
    /// <summary>
    /// Проверяет, можно ли сейчас выполнить это действие
    /// </summary>
    public abstract bool CanClaim(CardSystem cardSystem, PlayerInventory playerInventory, out string status);
    /// <summary>
    /// Пытается выдать награду игроку и сообщает, получилось ли ее забрать
    /// </summary>
    public abstract bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory);
    /// <summary>
    /// Создает данные награды из карты
    /// </summary>
    public static RewardData FromCard(CardData card)
    {
        return card != null ? new CardRewardData(card) : null;
    }
    /// <summary>
    /// Создает данные награды из предмета
    /// </summary>
    public static RewardData FromItem(ItemData item)
    {
        return item != null ? new ItemRewardData(item) : null;
    }
}
/// <summary>
/// Награда в виде карты, которую можно добавить игроку в руку
/// </summary>
[Serializable]
public sealed class CardRewardData : RewardData
{
    private readonly CardData cardData;
    /// <summary>
    /// Создает награду, которая добавляет карту в руку игрока
    /// </summary>
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
    /// <summary>
    /// Проверяет, можно ли сейчас выполнить это действие
    /// </summary>
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
    /// <summary>
    /// Пытается добавить карту-награду в руку игрока
    /// </summary>
    public override bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory)
    {
        return cardSystem != null && cardData != null && cardSystem.AddCard(cardData);
    }
}
/// <summary>
/// Награда в виде предмета, который можно экипировать игроку
/// </summary>
[Serializable]
public sealed class ItemRewardData : RewardData
{
    private readonly ItemData itemData;
    /// <summary>
    /// Создает награду, которая добавляет предмет в экипировку игрока
    /// </summary>
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
    /// <summary>
    /// Проверяет, можно ли сейчас выполнить это действие
    /// </summary>
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
    /// <summary>
    /// Пытается надеть предмет-награду на игрока
    /// </summary>
    public override bool TryClaim(CardSystem cardSystem, PlayerInventory playerInventory)
    {
        return playerInventory != null && itemData != null && playerInventory.TryEquip(itemData);
    }
}
