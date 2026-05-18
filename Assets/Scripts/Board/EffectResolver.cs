using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EffectResolver
{
    private PlayerStats playerStats;
    private PlayerInventory playerInventory;
    private CardSystem cardSystem;
    private SingleRewardSystem singleRewardSystem;
    private EventNotificationSystem eventNotificationSystem;
    private IReadOnlyList<CardData> possibleCommonCards;
    private IReadOnlyList<CardData> possibleRareCards;
    private IReadOnlyList<ItemData> possibleRareItems;

    public void Configure(
        PlayerStats newPlayerStats,
        PlayerInventory newPlayerInventory,
        CardSystem newCardSystem,
        SingleRewardSystem newSingleRewardSystem,
        EventNotificationSystem newEventNotificationSystem,
        IReadOnlyList<CardData> newPossibleCommonCards,
        IReadOnlyList<CardData> newPossibleRareCards,
        IReadOnlyList<ItemData> newPossibleRareItems)
    {
        playerStats = newPlayerStats;
        playerInventory = newPlayerInventory;
        cardSystem = newCardSystem;
        singleRewardSystem = newSingleRewardSystem;
        eventNotificationSystem = newEventNotificationSystem;
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
            var previousHp = playerStats.CurrentHp;
            playerStats.Heal(playerStats.MaxHp - previousHp);
            var restoredHp = playerStats.CurrentHp - previousHp;
            NotifyEffect(effect, restoredHp > 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect, restoredHp > 0 ? restoredHp : playerStats.MaxHp);
            Debug.Log($"{sourceName} fully healed the player.");
            return true;
        }

        var value = effect.Value;
        if (value > 0)
        {
            var previousHp = playerStats.CurrentHp;
            playerStats.Heal(value);
            var restoredHp = playerStats.CurrentHp - previousHp;
            NotifyEffect(effect, restoredHp > 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect, restoredHp > 0 ? restoredHp : value);

            Debug.Log($"{sourceName} restored {value} HP.");
            return true;
        }

        if (value < 0)
        {
            var damage = -value;
            if (playerInventory != null && playerInventory.TryBreakArmorForHpLoss())
            {
                NotifyEffect(effect, EffectNotificationStatus.Blocked);
                Debug.Log($"{sourceName} HP loss was prevented by armor.");
                return true;
            }

            var previousHp = playerStats.CurrentHp;
            playerStats.TakeDamage(damage);
            var lostHp = previousHp - playerStats.CurrentHp;
            NotifyEffect(effect, lostHp > 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect, lostHp > 0 ? -lostHp : value);

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
            NotifyEffect(EffectType.Level, value, EffectNotificationStatus.NoEffect);
            Debug.Log($"{sourceName} left level unchanged at {playerStats.Level}.");
            return true;
        }

        NotifyEffect(EffectType.Level, levelChange, EffectNotificationStatus.Success);
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
            NotifyEffect(effect, EffectNotificationStatus.Failed);
            onResolved?.Invoke();
            return false;
        }

        if (singleRewardSystem == null)
        {
            Debug.LogWarning($"{sourceName} cannot show card reward '{card.CardName}' because SingleRewardSystem is not assigned.");
            NotifyEffect(effect, EffectNotificationStatus.Failed);
            onResolved?.Invoke();
            return false;
        }

        if (!singleRewardSystem.ShowReward(RewardData.FromCard(card), onResolved, NotifyRewardClaimed))
        {
            Debug.LogWarning($"{sourceName} could not show card reward '{card.CardName}'.");
            NotifyEffect(effect, EffectNotificationStatus.Failed);
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
            NotifyEffect(effect, EffectNotificationStatus.Failed);
            onResolved?.Invoke();
            return false;
        }

        if (singleRewardSystem == null)
        {
            Debug.LogWarning($"{sourceName} cannot show item reward '{item.ItemName}' because SingleRewardSystem is not assigned.");
            NotifyEffect(effect, EffectNotificationStatus.Failed);
            onResolved?.Invoke();
            return false;
        }

        if (!singleRewardSystem.ShowReward(RewardData.FromItem(item), onResolved, NotifyRewardClaimed))
        {
            Debug.LogWarning($"{sourceName} could not show item reward '{item.ItemName}'.");
            NotifyEffect(effect, EffectNotificationStatus.Failed);
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
                NotifyEffect(effect, removedAny ? EffectNotificationStatus.Success : EffectNotificationStatus.Failed, removedAny ? i : removeCount);
                return removedAny;
            }

            removedAny = true;
        }

        Debug.Log($"{sourceName} removed {removeCount} random {rarity} card.");
        NotifyEffect(effect, EffectNotificationStatus.Success, removeCount);
        return true;
    }

    private void NotifyRewardClaimed(RewardData reward)
    {
        if (reward == null)
            return;

        switch (reward.RewardType)
        {
            case RewardType.Card:
                NotifyEffect(EffectType.GiveCard, 1, EffectNotificationStatus.Success);
                break;
            case RewardType.Item:
                NotifyEffect(EffectType.GiveItem, 1, EffectNotificationStatus.Success);
                break;
        }
    }

    private void NotifyEffect(EffectData effect, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effect, status);
    }

    private void NotifyEffect(EffectData effect, EffectNotificationStatus status, int displayValue)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effect, status, displayValue);
    }

    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
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
