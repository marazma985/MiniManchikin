using System.Collections.Generic;
using UnityEngine;

public sealed class EffectResolver
{
    private PlayerStats playerStats;
    private PlayerInventory playerInventory;
    private CardSystem cardSystem;
    private IReadOnlyList<CardData> possibleCommonCards;

    public void Configure(PlayerStats newPlayerStats, PlayerInventory newPlayerInventory, CardSystem newCardSystem, IReadOnlyList<CardData> newPossibleCommonCards)
    {
        playerStats = newPlayerStats;
        playerInventory = newPlayerInventory;
        cardSystem = newCardSystem;
        possibleCommonCards = newPossibleCommonCards;
    }

    public bool TryApply(EffectData effect, string sourceName)
    {
        if (effect == null)
        {
            Debug.LogWarning($"{sourceName} has a missing effect.");
            return false;
        }

        switch (effect.EffectType)
        {
            case EffectType.HpRestore:
                return ApplyHpRestore(effect.Value, sourceName);
            case EffectType.Level:
                return ApplyLevel(effect.Value, sourceName);
            case EffectType.GiveCard:
                return ApplyGiveCard(sourceName);
            case EffectType.RemoveCard:
                return ApplyRemoveCard(effect, sourceName);
            default:
                Debug.LogWarning($"{sourceName} has unsupported effect type '{effect.EffectType}'.");
                return false;
        }
    }

    private bool ApplyHpRestore(int value, string sourceName)
    {
        if (playerStats == null)
        {
            Debug.LogWarning($"{sourceName} requires PlayerStats for HP effect.");
            return false;
        }

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

    private bool ApplyGiveCard(string sourceName)
    {
        if (cardSystem == null)
        {
            Debug.LogWarning($"{sourceName} requires CardSystem to give a card.");
            return false;
        }

        var card = GetRandomCommonCard();
        if (card == null)
        {
            Debug.LogWarning($"{sourceName} cannot give a common card because the common card pool is empty.");
            return false;
        }

        if (!cardSystem.AddCard(card))
        {
            Debug.LogWarning($"{sourceName} could not give card '{card.CardName}'.");
            return false;
        }

        Debug.Log($"{sourceName} gave common card: {card.CardName}.");
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

    private CardData GetRandomCommonCard()
    {
        if (possibleCommonCards == null)
            return null;

        var validCards = new List<CardData>();
        for (var i = 0; i < possibleCommonCards.Count; i++)
        {
            var card = possibleCommonCards[i];
            if (card != null && card.Rarity == Rarity.Common)
                validCards.Add(card);
        }

        return validCards.Count == 0 ? null : validCards[Random.Range(0, validCards.Count)];
    }
}
