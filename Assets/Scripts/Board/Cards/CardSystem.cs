using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Хранит карты игрока и решает, можно ли применить карту в текущий момент игры
/// </summary>

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

    /// <summary>
    /// Карты, которые сейчас находятся в руке игрока
    /// </summary>
    public IReadOnlyList<CardData> Hand => hand;
    /// <summary>
    /// Максимальное количество карт в руке
    /// </summary>
    public int MaxCards => MaxHandSize;
    /// <summary>
    /// Заменяет руку игрока списком карт из сохранения
    /// </summary>
    /// <param name="cards">Список карт, который нужно показать или сохранить в руке</param>
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
    /// <summary>
    /// Добавляет карту в руку игрока, если в руке есть место
    /// </summary>
    /// <param name="card">Карта, которую нужно добавить в руку</param>
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
    /// <summary>
    /// Удаляет указанную карту из руки игрока
    /// </summary>
    /// <param name="card">Карта, которую нужно удалить из руки</param>
    public bool RemoveCard(CardData card)
    {
        if (card == null || !hand.Remove(card))
            return false;

        NotifyHandChanged();
        return true;
    }
    /// <summary>
    /// Удаляет случайную карту указанной редкости из руки игрока
    /// </summary>
    /// <param name="rarity">Редкость карты для поиска или удаления</param>
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
    /// <summary>
    /// Пытается применить выбранную карту из руки игрока
    /// </summary>
    /// <param name="card">Карта из руки, которую игрок пытается применить</param>
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
    /// <summary>
    /// Ограничивает размер руки и убирает невозможные значения в инспекторе
    /// </summary>
    private void OnValidate()
    {
        if (hand == null)
            hand = new List<CardData>(MaxHandSize);

        if (hand.Count <= MaxHandSize)
            return;

        hand.RemoveRange(MaxHandSize, hand.Count - MaxHandSize);
    }
    /// <summary>
    /// Запускает начальную настройку после загрузки сцены
    /// </summary>
    private void Start()
    {
        NotifyHandChanged();
    }
    /// <summary>
    /// Сообщает интерфейсу, что руку карт нужно перерисовать
    /// </summary>
    private void NotifyHandChanged()
    {
        OnHandChanged?.Invoke(hand);
    }
    /// <summary>
    /// Проверяет карту, применяет ее эффекты и обновляет бой при необходимости
    /// </summary>
    /// <param name="card">Карта, которую нужно проверить и применить</param>
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
    /// <summary>
    /// Проверяет, разрешено ли использовать карту сейчас: на поле, в бою или везде
    /// </summary>
    /// <param name="card">Карта, для которой проверяется место использования</param>
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
    /// <summary>
    /// Проверяет, можно ли применить все эффекты выбранной карты
    /// </summary>
    /// <param name="card">Карта, эффекты которой нужно проверить</param>
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
    /// <summary>
    /// Проверяет, можно ли применить один эффект выбранной карты
    /// </summary>
    /// <param name="card">Карта, к которой относится проверяемый эффект</param>
    /// <param name="effect">Эффект карты, который нужно проверить</param>
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
    /// <summary>
    /// Применяет все эффекты выбранной карты по очереди
    /// </summary>
    /// <param name="card">Карта, эффекты которой нужно применить</param>
    private bool ApplyAllEffects(CardData card)
    {
        for (var i = 0; i < card.Effects.Count; i++)
        {
            if (!ApplyEffect(card.Effects[i]))
                return false;
        }

        return true;
    }
    /// <summary>
    /// Применяет один эффект карты к игроку, бою или полю
    /// </summary>
    /// <param name="effect">Эффект карты, который нужно применить</param>
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
    /// <summary>
    /// Проверяет, может ли карта сейчас переместить игрока к нужной клетке
    /// </summary>
    /// <param name="card">Карта, которая пытается переместить игрока</param>
    /// <param name="effect">Эффект перемещения, который нужно проверить</param>
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

        var targetTileType = effect.TargetTileType;
        if (targetTileType == TileType.None)
        {
            Debug.LogWarning($"Card '{card.CardName}' movement effect has no target tile.");
            return false;
        }

        if (boardManager.TryGetForwardDistanceToNearestTile(targetTileType, out _))
            return true;

        Debug.LogWarning($"Card '{card.CardName}' could not find a {targetTileType} tile.");
        return false;
    }
    /// <summary>
    /// Перемещает игрока на нужную клетку по эффекту карты
    /// </summary>
    /// <param name="effect">Эффект перемещения, по которому выбирается целевая клетка</param>
    private bool ApplyChangePosition(EffectData effect)
    {
        if (!boardManager.TryGetForwardDistanceToNearestTile(effect.TargetTileType, out var steps))
            return false;

        return turnSystem.TryMoveFixedSteps(steps);
    }
    /// <summary>
    /// Показывает подсказку о результате применения эффекта карты
    /// </summary>
    /// <param name="effect">Эффект карты, о котором нужно показать подсказку</param>
    /// <param name="status">Статус применения эффекта для подсказки</param>
    private void NotifyEffect(EffectData effect, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effect, status);
    }
    /// <summary>
    /// Показывает подсказку о результате эффекта карты с уже посчитанным числом
    /// </summary>
    /// <param name="effect">Эффект карты, о котором нужно показать подсказку</param>
    /// <param name="status">Статус применения эффекта для подсказки</param>
    /// <param name="displayValue">Число, которое нужно показать в подсказке</param>
    private void NotifyEffect(EffectData effect, EffectNotificationStatus status, int displayValue)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effect, status, displayValue);
    }
}
