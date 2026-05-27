using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Генерирует варианты наград после боя, показывает окно выбора, восстанавливает награды и выдает выбранный приз
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
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public bool ShowBattleRewards(Action onRewardAccepted)
    {
        if (rewardModalView == null)
        {
            Debug.LogWarning("RewardSystem requires RewardModalView.");
            return false;
        }

        currentRewards.Clear();
        // Конкретные варианты награды генерируются один раз и сохраняются, поэтому Продолжить показывает тот же набор
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
    /// Восстанавливает состояние из сохраненных данных
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
    /// Регистрирует данные или подписки, которые нужны другим системам
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
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
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
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
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
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private RewardData CreateRandomReward()
    {
        var hasCards = HasAnyCardReward();
        var hasItems = HasAnyItemReward();

        // Пулы карт и предметов настраиваются независимо, поэтому случайный выбор сначала проверяет какие пулы доступны
        if (!hasCards && !hasItems)
            return null;

        if (hasCards && hasItems)
            return UnityEngine.Random.Range(0, 2) == 0 ? CreateRandomCardReward() : CreateRandomItemReward();

        return hasCards ? CreateRandomCardReward() : CreateRandomItemReward();
    }
    /// <summary>
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private RewardData CreateRandomCardReward()
    {
        return RewardData.FromCard(GetRandomCardReward());
    }
    /// <summary>
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private RewardData CreateRandomItemReward()
    {
        return RewardData.FromItem(GetRandomItemReward());
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
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
    /// Возвращает сохраненное или рассчитанное значение
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
    /// Проверяет, есть ли нужное состояние или данные
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
    /// Проверяет, есть ли нужное состояние или данные
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
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleRewardSelected(RewardData reward)
    {
        if (!TryClaimReward(reward))
            return;

        CompleteRewardFlow();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCloseRequested()
    {
        Debug.Log("Reward skipped.");
        CompleteRewardFlow();
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода CompleteRewardFlow
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
    /// Пытается выполнить действие и возвращает, получилось ли это сделать
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
    /// Сообщает подписчикам, что состояние изменилось
    /// </summary>
    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (cardRewards == null)
            cardRewards = new List<CardData>();
        if (itemRewards == null)
            itemRewards = new List<ItemData>();
    }
}
