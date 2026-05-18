using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EffectResolver
{
    private PlayerStats playerStats;
    private PlayerInventory playerInventory;
    private CardSystem cardSystem;
    private SingleRewardSystem singleRewardSystem;
    private IReadOnlyList<CardData> possibleCommonCards;
    private IReadOnlyList<CardData> possibleRareCards;
    private IReadOnlyList<ItemData> possibleRareItems;

    public void Configure(
        PlayerStats newPlayerStats,
        PlayerInventory newPlayerInventory,
        CardSystem newCardSystem,
        SingleRewardSystem newSingleRewardSystem,
        IReadOnlyList<CardData> newPossibleCommonCards,
        IReadOnlyList<CardData> newPossibleRareCards,
        IReadOnlyList<ItemData> newPossibleRareItems)
    {
        playerStats = newPlayerStats;
        playerInventory = newPlayerInventory;
        cardSystem = newCardSystem;
        singleRewardSystem = newSingleRewardSystem;
        possibleCommonCards = newPossibleCommonCards;
        possibleRareCards = newPossibleRareCards;
        possibleRareItems = newPossibleRareItems;
    }

    public bool TryApply(EffectData effect, string sourceName)
    {
        return TryApply(effect, sourceName, null);
    }

    public bool TryApply(EffectData effect, string sourceName, Action onResolved)
    {
        if (effect == null)
        {
            Debug.LogWarning($"{sourceName} has a missing effect.");
            onResolved?.Invoke();
            return false;
        }

        bool result;
        switch (effect.EffectType)
        {
            case EffectType.HpRestore:
                result = ApplyHpRestore(effect, sourceName);
                break;
            case EffectType.Level:
                result = ApplyLevel(effect.Value, sourceName);
                break;
            case EffectType.GiveCard:
                return ApplyGiveCard(effect, sourceName, onResolved);
            case EffectType.RemoveCard:
                result = ApplyRemoveCard(effect, sourceName);
                break;
            case EffectType.GiveItem:
                return ApplyGiveItem(effect, sourceName, onResolved);
            default:
                Debug.LogWarning($"{sourceName} has unsupported effect type '{effect.EffectType}'.");
                result = false;
                break;
        }

        onResolved?.Invoke();
        return result;
    }

    private bool ApplyHpRestore(EffectData effect, string sourceName)
    {
        if (playerStats == null)
        {
            Debug.LogWarning($"{sourceName} requires PlayerStats for HP effect.");
            return false;
        }

        if (effect.RestoreToFull)
        {
            playerStats.Heal(playerStats.MaxHp - playerStats.CurrentHp);
            Debug.Log($"{sourceName} fully healed the player.");
            return true;
        }

        var value = effect.Value;
        if (value > 0)
        {
            playerStats.Heal(value);
            Debug.Log($"{sourceName} restored {value} HP.");
            return true;
        }

        if (value < 0)
        {
            var damage = -value;
            if (playerInventory != null && playerInventory.TryBreakArmorForHpLoss())
            {
                Debug.Log($"{sourceName} HP loss was prevented by armor.");
                return true;
            }

            playerStats.TakeDamage(damage);
            Debug.Log($"{sourceName} dealt {damage} HP damage.");
            return true;
        }

        Debug.LogWarning($"{sourceName} HP effect has zero value.");
        return false;
    }

    private bool ApplyLevel(int value, string sourceName)
    {
        if (playerStats == null)
        {
            Debug.LogWarning($"{sourceName} requires PlayerStats for level effect.");
            return false;
        }

        if (value == 0)
        {
            Debug.LogWarning($"{sourceName} level effect has zero value.");
            return false;
        }

        var previousLevel = playerStats.Level;
        playerStats.SetLevel(previousLevel + value);
        var levelChange = playerStats.Level - previousLevel;
        if (levelChange == 0)
        {
            Debug.Log($"{sourceName} left level unchanged at {playerStats.Level}.");
            return true;
        }

        Debug.Log(levelChange > 0 ? $"{sourceName} added {levelChange} level." : $"{sourceName} removed {-levelChange} level.");
        return true;
    }

    private bool ApplyGiveCard(EffectData effect, string sourceName, Action onResolved)
    {
        var rarity = effect.RarityFilter;
        var card = GetRandomCard(rarity);
        if (card == null)
        {
            Debug.LogWarning($"{sourceName} cannot give a {rarity} card because the card pool is empty.");
            onResolved?.Invoke();
            return false;
        }

        if (singleRewardSystem == null)
        {
            Debug.LogWarning($"{sourceName} cannot show card reward '{card.CardName}' because SingleRewardSystem is not assigned.");
            onResolved?.Invoke();
            return false;
        }

        if (!singleRewardSystem.ShowReward(RewardData.FromCard(card), onResolved))
        {
            Debug.LogWarning($"{sourceName} could not show card reward '{card.CardName}'.");
            onResolved?.Invoke();
            return false;
        }

        Debug.Log($"{sourceName} offered {rarity} card reward: {card.CardName}.");
        return true;
    }

    private bool ApplyGiveItem(EffectData effect, string sourceName, Action onResolved)
    {
        var rarity = effect.RarityFilter;
        var item = GetRandomItem(rarity);
        if (item == null)
        {
            Debug.LogWarning($"{sourceName} cannot give a {rarity} item because the item pool is empty.");
            onResolved?.Invoke();
            return false;
        }

        if (singleRewardSystem == null)
        {
            Debug.LogWarning($"{sourceName} cannot show item reward '{item.ItemName}' because SingleRewardSystem is not assigned.");
            onResolved?.Invoke();
            return false;
        }

        if (!singleRewardSystem.ShowReward(RewardData.FromItem(item), onResolved))
        {
            Debug.LogWarning($"{sourceName} could not show item reward '{item.ItemName}'.");
            onResolved?.Invoke();
            return false;
        }

        Debug.Log($"{sourceName} offered {rarity} item reward: {item.ItemName}.");
        return true;
    }

    private bool ApplyRemoveCard(EffectData effect, string sourceName)
    {
        if (cardSystem == null)
        {
            Debug.LogWarning($"{sourceName} requires CardSystem to remove a card.");
            return false;
        }

        var rarity = effect.RarityFilter;
        var removeCount = Mathf.Max(1, effect.Value);
        var removedAny = false;

        for (var i = 0; i < removeCount; i++)
        {
            if (!cardSystem.RemoveRandomCard(rarity))
            {
                Debug.LogWarning($"{sourceName} could not remove a random {rarity} card.");
                return removedAny;
            }

            removedAny = true;
        }

        Debug.Log($"{sourceName} removed {removeCount} random {rarity} card.");
        return true;
    }

    private CardData GetRandomCard(Rarity rarity)
    {
        var cards = rarity == Rarity.Rare ? possibleRareCards : possibleCommonCards;
        if (cards == null)
            return null;

        var validCards = new List<CardData>();
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card != null && card.Rarity == rarity)
                validCards.Add(card);
        }

        return validCards.Count == 0 ? null : validCards[UnityEngine.Random.Range(0, validCards.Count)];
    }

    private ItemData GetRandomItem(Rarity rarity)
    {
        var items = rarity == Rarity.Rare ? possibleRareItems : null;
        if (items == null)
            return null;

        var validItems = new List<ItemData>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item != null && item.Rarity == rarity)
                validItems.Add(item);
        }

        return validItems.Count == 0 ? null : validItems[UnityEngine.Random.Range(0, validItems.Count)];
    }
}
