using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardSystem : MonoBehaviour
{
    private const int MaxHandSize = 3;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BattleSystem battleSystem;
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
        if (card == null || hand.Count >= MaxHandSize)
            return false;

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
            battleSystem.RefreshCurrentBattleView($"Card applied: {card.CardName}");

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
                playerStats.Heal(effect.Value);
                return true;
            case EffectType.Level:
                playerStats.SetLevel(playerStats.Level + effect.Value);
                return true;
            case EffectType.Power:
                return battleSystem.AddTemporaryCardPower(effect.Value);
            case EffectType.EscapeBonus:
                return battleSystem.AddTemporaryEscapeBonus(effect.Value);
            default:
                return false;
        }
    }
}
