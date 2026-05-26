using System;
using System.Collections.Generic;
using UnityEngine;

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

    public bool ShowBattleRewards(Action onRewardAccepted)
    {
        if (rewardModalView == null)
        {
            Debug.LogWarning("RewardSystem requires RewardModalView.");
            return false;
        }

        currentRewards.Clear();
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
        return true;
    }

    private void OnEnable()
    {
        if (rewardModalView != null)
        {
            rewardModalView.RewardSelected += HandleRewardSelected;
            rewardModalView.CloseRequested += HandleCloseRequested;
        }
    }

    private void OnDisable()
    {
        if (rewardModalView != null)
        {
            rewardModalView.RewardSelected -= HandleRewardSelected;
            rewardModalView.CloseRequested -= HandleCloseRequested;
        }
    }

    private RewardData CreateRandomReward()
    {
        var hasCards = HasAnyCardReward();
        var hasItems = HasAnyItemReward();

        if (!hasCards && !hasItems)
            return null;

        if (hasCards && hasItems)
            return UnityEngine.Random.Range(0, 2) == 0 ? CreateRandomCardReward() : CreateRandomItemReward();

        return hasCards ? CreateRandomCardReward() : CreateRandomItemReward();
    }

    private RewardData CreateRandomCardReward()
    {
        return RewardData.FromCard(GetRandomCardReward());
    }

    private RewardData CreateRandomItemReward()
    {
        return RewardData.FromItem(GetRandomItemReward());
    }

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

    private void HandleRewardSelected(RewardData reward)
    {
        if (!TryClaimReward(reward))
            return;

        CompleteRewardFlow();
    }

    private void HandleCloseRequested()
    {
        Debug.Log("Reward skipped.");
        CompleteRewardFlow();
    }

    private void CompleteRewardFlow()
    {
        if (rewardModalView != null)
            rewardModalView.Hide();

        currentRewards.Clear();

        var onAccepted = rewardAccepted;
        rewardAccepted = null;
        onAccepted?.Invoke();
    }

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

    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
    }

    private void OnValidate()
    {
        if (cardRewards == null)
            cardRewards = new List<CardData>();
        if (itemRewards == null)
            itemRewards = new List<ItemData>();
    }
}
