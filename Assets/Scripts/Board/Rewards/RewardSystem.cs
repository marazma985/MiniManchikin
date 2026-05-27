using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Создает варианты наград после боя и выдает выбранную награду игроку
/// </summary>

public sealed class RewardSystem : MonoBehaviour
{
    private const int RewardOptionCount = 3;

    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private RewardModalView rewardModalView;
    [SerializeField] private EventNotificationSystem eventNotificationSystem;
    [SerializeField] private List<CardData> cardRewards = new List<CardData>();
    [SerializeField] private List<ItemData> itemRewards = new List<ItemData>();

    private readonly List<RewardData> currentRewards = new List<RewardData>(RewardOptionCount);
    private Action rewardAccepted;

    public event Action RewardStateChanged;

    public IReadOnlyList<RewardData> CurrentRewards => currentRewards;
    /// <summary>
    /// Показывает игроку несколько наград после победы в бою
    /// </summary>
    public bool ShowBattleRewards(Action onRewardAccepted)
    {
        if (rewardModalView == null)
        {
            Debug.LogWarning("RewardSystem requires RewardModalView.");
            return false;
        }

        currentRewards.Clear();
        // Варианты награды создаются один раз и сохраняются, чтобы после Продолжить игрок видел тот же выбор
        for (var i = 0; i < RewardOptionCount; i++)
        {
            var reward = CreateRandomReward();
            if (reward != null)
                currentRewards.Add(reward);
        }

        if (currentRewards.Count == 0)
        {
            Debug.LogWarning("RewardSystem has no rewards to show.");
            return false;
        }

        rewardAccepted = onRewardAccepted;
        rewardModalView.Show(currentRewards);
        RewardStateChanged?.Invoke();
        return true;
    }
    /// <summary>
    /// Возвращает на экран те же награды после продолжения сохранения
    /// </summary>
    public bool RestoreBattleRewards(IReadOnlyList<RewardData> rewards, Action onRewardAccepted)
    {
        if (rewardModalView == null || rewards == null || rewards.Count == 0)
            return false;

        currentRewards.Clear();
        for (var i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] != null)
                currentRewards.Add(rewards[i]);
        }

        if (currentRewards.Count == 0)
            return false;

        rewardAccepted = onRewardAccepted;
        rewardModalView.Show(currentRewards);
        RewardStateChanged?.Invoke();
        return true;
    }
    /// <summary>
    /// Передает системе сохранений данные, которые можно будет найти по id
    /// </summary>
    public void RegisterSaveContent(GameSaveContentResolver resolver)
    {
        if (resolver == null)
            return;

        if (cardRewards != null)
        {
            for (var i = 0; i < cardRewards.Count; i++)
                resolver.AddCard(cardRewards[i]);
        }

        if (itemRewards != null)
        {
            for (var i = 0; i < itemRewards.Count; i++)
                resolver.AddItem(itemRewards[i]);
        }
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (rewardModalView != null)
        {
            rewardModalView.RewardSelected += HandleRewardSelected;
            rewardModalView.CloseRequested += HandleCloseRequested;
        }
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (rewardModalView != null)
        {
            rewardModalView.RewardSelected -= HandleRewardSelected;
            rewardModalView.CloseRequested -= HandleCloseRequested;
        }
    }
    /// <summary>
    /// Выбирает случайную награду из доступных карт и предметов
    /// </summary>
    private RewardData CreateRandomReward()
    {
        var hasCards = HasAnyCardReward();
        var hasItems = HasAnyItemReward();

        // Карты и предметы могут быть настроены отдельно, поэтому сначала проверяется что вообще доступно
        if (!hasCards && !hasItems)
            return null;

        if (hasCards && hasItems)
            return UnityEngine.Random.Range(0, 2) == 0 ? CreateRandomCardReward() : CreateRandomItemReward();

        return hasCards ? CreateRandomCardReward() : CreateRandomItemReward();
    }
    /// <summary>
    /// Создает случайную награду-карту после победы
    /// </summary>
    private RewardData CreateRandomCardReward()
    {
        return RewardData.FromCard(GetRandomCardReward());
    }
    /// <summary>
    /// Создает случайную награду-предмет после победы
    /// </summary>
    private RewardData CreateRandomItemReward()
    {
        return RewardData.FromItem(GetRandomItemReward());
    }
    /// <summary>
    /// Выбирает случайную карту для награды
    /// </summary>
    private CardData GetRandomCardReward()
    {
        if (cardRewards == null)
            return null;

        var validCards = new List<CardData>();
        for (var i = 0; i < cardRewards.Count; i++)
        {
            if (cardRewards[i] != null)
                validCards.Add(cardRewards[i]);
        }

        return validCards.Count == 0 ? null : validCards[UnityEngine.Random.Range(0, validCards.Count)];
    }
    /// <summary>
    /// Выбирает случайный предмет для награды
    /// </summary>
    private ItemData GetRandomItemReward()
    {
        if (itemRewards == null)
            return null;

        var validItems = new List<ItemData>();
        for (var i = 0; i < itemRewards.Count; i++)
        {
            if (itemRewards[i] != null)
                validItems.Add(itemRewards[i]);
        }

        return validItems.Count == 0 ? null : validItems[UnityEngine.Random.Range(0, validItems.Count)];
    }
    /// <summary>
    /// Проверяет, есть ли хотя бы одна карта, которую можно выдать как награду
    /// </summary>
    private bool HasAnyCardReward()
    {
        if (cardRewards == null)
            return false;

        for (var i = 0; i < cardRewards.Count; i++)
        {
            if (cardRewards[i] != null)
                return true;
        }

        return false;
    }
    /// <summary>
    /// Проверяет, есть ли хотя бы один предмет, который можно выдать как награду
    /// </summary>
    private bool HasAnyItemReward()
    {
        if (itemRewards == null)
            return false;

        for (var i = 0; i < itemRewards.Count; i++)
        {
            if (itemRewards[i] != null)
                return true;
        }

        return false;
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleRewardSelected(RewardData reward)
    {
        if (!TryClaimReward(reward))
            return;

        CompleteRewardFlow();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleCloseRequested()
    {
        Debug.Log("Reward skipped.");
        CompleteRewardFlow();
    }
    /// <summary>
    /// Закрывает награду и возвращает игру к следующему ходу
    /// </summary>
    private void CompleteRewardFlow()
    {
        if (rewardModalView != null)
            rewardModalView.Hide();

        currentRewards.Clear();

        var onAccepted = rewardAccepted;
        rewardAccepted = null;
        onAccepted?.Invoke();
        RewardStateChanged?.Invoke();
    }
    /// <summary>
    /// Пытается забрать выбранную награду и показать причину, если это невозможно
    /// </summary>
    private bool TryClaimReward(RewardData reward)
    {
        if (reward == null)
        {
            Debug.LogWarning("Cannot claim a missing reward.");
            return false;
        }

        if (!reward.CanClaim(cardSystem, playerInventory, out var status))
        {
            rewardModalView?.ShowStatus(status);
            NotifyEffect(reward.ClaimEffectType, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Reward '{reward.DisplayName}' was not claimed: {status}");
            return false;
        }

        if (!reward.TryClaim(cardSystem, playerInventory))
        {
            var failureStatus = string.IsNullOrEmpty(status) ? "Награда не получена" : status;
            rewardModalView?.ShowStatus(failureStatus);
            NotifyEffect(reward.ClaimEffectType, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Reward '{reward.DisplayName}' was not claimed.");
            return false;
        }

        Debug.Log($"Reward claimed: {reward.DisplayName}.");
        NotifyEffect(reward.ClaimEffectType, 1, EffectNotificationStatus.Success);
        return true;
    }
    /// <summary>
    /// Показывает подсказку о том, что награду нельзя забрать или эффект сработал
    /// </summary>
    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (cardRewards == null)
            cardRewards = new List<CardData>();
        if (itemRewards == null)
            itemRewards = new List<ItemData>();
    }
}
