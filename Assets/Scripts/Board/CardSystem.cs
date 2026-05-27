using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardSystem : MonoBehaviour
{
    private const int MaxHandSize = 3;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private EventNotificationSystem eventNotificationSystem;
    [SerializeField] private List<CardData> hand = new List<CardData>(MaxHandSize);

    public event Action<IReadOnlyList<CardData>> OnHandChanged;

    public IReadOnlyList<CardData> Hand => hand;
    public int MaxCards => MaxHandSize;

    public void SetHand(List<CardData> cards)
    {
        hand.Clear();

        if (cards != null)
        {
            for (var i = 0; i < cards.Count && hand.Count < MaxHandSize; i++)
            {
                if (cards[i] != null)
                    hand.Add(cards[i]);
            }
        }

        NotifyHandChanged();
    }

    public bool AddCard(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot add a null card to hand.");
            return false;
        }

        if (hand.Count >= MaxHandSize)
        {
            Debug.LogWarning($"Cannot add card '{card.CardName}'. Hand is full ({MaxHandSize} cards).");
            return false;
        }

        hand.Add(card);
        NotifyHandChanged();
        return true;
    }

    public bool RemoveCard(CardData card)
    {
        if (card == null || !hand.Remove(card))
            return false;

        NotifyHandChanged();
        return true;
    }

    public bool RemoveRandomCard(Rarity rarity)
    {
        var matchingCards = new List<CardData>();
        for (var i = 0; i < hand.Count; i++)
        {
            var card = hand[i];
            if (card != null && card.Rarity == rarity)
                matchingCards.Add(card);
        }

        if (matchingCards.Count == 0)
        {
            Debug.LogWarning($"Cannot remove random {rarity} card. No matching cards in hand.");
            return false;
        }

        var cardToRemove = matchingCards[UnityEngine.Random.Range(0, matchingCards.Count)];
        if (!RemoveCard(cardToRemove))
            return false;

        Debug.Log($"Removed random {rarity} card: {cardToRemove.CardName}.");
        return true;
    }

    public bool UseCard(CardData card)
    {
        if (card == null || !hand.Contains(card))
            return false;

        if (!ResolveCard(card))
            return false;

        Debug.Log($"Card used: {card.CardName}");
        RemoveCard(card);
        return true;
    }

    private void OnValidate()
    {
        if (hand == null)
            hand = new List<CardData>(MaxHandSize);

        if (hand.Count <= MaxHandSize)
            return;

        hand.RemoveRange(MaxHandSize, hand.Count - MaxHandSize);
    }

    private void Start()
    {
        NotifyHandChanged();
    }

    private void NotifyHandChanged()
    {
        OnHandChanged?.Invoke(hand);
    }

    private bool ResolveCard(CardData card)
    {
        if (!CanUseCardInCurrentContext(card))
            return false;

        if (!CanApplyAllEffects(card))
            return false;

        if (!ApplyAllEffects(card))
            return false;

        if (battleSystem != null && battleSystem.IsBattleActive)
            battleSystem.RefreshCurrentBattleView($"Карта применена: {card.CardName}");

        return true;
    }

    private bool CanUseCardInCurrentContext(CardData card)
    {
        var isBattleActive = battleSystem != null && battleSystem.IsBattleActive;
        switch (card.UsageContext)
        {
            case UsageContext.BattleOnly:
                if (isBattleActive)
                    return true;

                Debug.LogWarning($"Card '{card.CardName}' can only be used during battle.");
                return false;
            case UsageContext.BoardOnly:
                if (!isBattleActive)
                    return true;

                Debug.LogWarning($"Card '{card.CardName}' can only be used outside battle.");
                return false;
            case UsageContext.Anywhere:
                return true;
            default:
                Debug.LogWarning($"Card '{card.CardName}' has unsupported usage context '{card.UsageContext}'.");
                return false;
        }
    }

    private bool CanApplyAllEffects(CardData card)
    {
        if (card.Effects == null || card.Effects.Count == 0)
        {
            Debug.LogWarning($"Card '{card.CardName}' has no effects.");
            return false;
        }

        for (var i = 0; i < card.Effects.Count; i++)
        {
            if (!CanApplyEffect(card, card.Effects[i]))
                return false;
        }

        return true;
    }

    private bool CanApplyEffect(CardData card, EffectData effect)
    {
        if (effect == null)
        {
            Debug.LogWarning($"Card '{card.CardName}' has a missing effect.");
            return false;
        }

        switch (effect.EffectType)
        {
            case EffectType.HpRestore:
                if (playerStats != null && effect.Value > 0)
                    return true;

                Debug.LogWarning($"Card '{card.CardName}' cannot apply HP restore effect.");
                return false;
            case EffectType.Level:
                if (playerStats != null && effect.Value != 0)
                    return true;

                Debug.LogWarning($"Card '{card.CardName}' cannot apply level effect.");
                return false;
            case EffectType.Power:
            case EffectType.EscapeBonus:
                if (battleSystem != null && battleSystem.IsBattleActive && effect.Value > 0)
                    return true;

                Debug.LogWarning($"Card '{card.CardName}' effect '{effect.EffectType}' can only be applied during battle.");
                return false;
            case EffectType.ChangePosition:
                return CanApplyChangePosition(card, effect);
            default:
                Debug.LogWarning($"Card '{card.CardName}' has unsupported effect type '{effect.EffectType}'.");
                return false;
        }
    }

    private bool ApplyAllEffects(CardData card)
    {
        for (var i = 0; i < card.Effects.Count; i++)
        {
            if (!ApplyEffect(card.Effects[i]))
                return false;
        }

        return true;
    }

    private bool ApplyEffect(EffectData effect)
    {
        switch (effect.EffectType)
        {
            case EffectType.HpRestore:
                var previousHp = playerStats.CurrentHp;
                playerStats.Heal(effect.Value);
                var restoredHp = playerStats.CurrentHp - previousHp;
                NotifyEffect(effect, restoredHp > 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect, restoredHp > 0 ? restoredHp : effect.Value);
                return true;
            case EffectType.Level:
                var previousLevel = playerStats.Level;
                playerStats.SetLevel(playerStats.Level + effect.Value);
                var levelChange = playerStats.Level - previousLevel;
                NotifyEffect(effect, levelChange != 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect, levelChange != 0 ? levelChange : effect.Value);
                return true;
            case EffectType.Power:
                if (!battleSystem.AddTemporaryCardPower(effect.Value))
                    return false;

                NotifyEffect(effect, EffectNotificationStatus.Success);
                return true;
            case EffectType.EscapeBonus:
                if (!battleSystem.AddTemporaryEscapeBonus(effect.Value))
                    return false;

                NotifyEffect(effect, EffectNotificationStatus.Success);
                return true;
            case EffectType.ChangePosition:
                if (!ApplyChangePosition(effect))
                    return false;

                NotifyEffect(effect, EffectNotificationStatus.Success);
                return true;
            default:
                return false;
        }
    }

    private bool CanApplyChangePosition(CardData card, EffectData effect)
    {
        if (battleSystem != null && battleSystem.IsBattleActive)
        {
            Debug.LogWarning($"Card '{card.CardName}' cannot move the player during battle.");
            return false;
        }

        if (turnSystem == null || boardManager == null)
        {
            Debug.LogWarning($"Card '{card.CardName}' requires TurnSystem and BoardManager for movement.");
            return false;
        }

        if (!turnSystem.CanRoll)
        {
            Debug.LogWarning($"Card '{card.CardName}' can only move the player while waiting for a roll.");
            return false;
        }

        if (!effect.UseNearestMatchingTile)
        {
            Debug.LogWarning($"Card '{card.CardName}' movement effect requires nearest matching tile targeting.");
            return false;
        }

        if (boardManager.TryGetForwardDistanceToNearestTileType(effect.TargetTileType, out _))
            return true;

        Debug.LogWarning($"Card '{card.CardName}' could not find a {effect.TargetTileType} tile.");
        return false;
    }

    private bool ApplyChangePosition(EffectData effect)
    {
        if (!boardManager.TryGetForwardDistanceToNearestTileType(effect.TargetTileType, out var steps))
            return false;

        return turnSystem.TryMoveFixedSteps(steps);
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
}
