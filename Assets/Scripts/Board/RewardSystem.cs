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
        var card = GetRandomCardReward();
        return RewardData.FromCard(card);
    }

    private RewardData CreateRandomItemReward()
    {
        var item = GetRandomItemReward();
        return RewardData.FromItem(item);
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

        switch (reward.RewardType)
        {
            case RewardType.Card:
                return TryClaimCardReward(reward);
            case RewardType.Item:
                return TryClaimItemReward(reward);
            default:
                rewardModalView?.ShowStatus("Неподдерживаемая награда");
                Debug.LogWarning($"Unsupported reward type '{reward.RewardType}'.");
                return false;
        }
    }

    private bool TryClaimCardReward(RewardData reward)
    {
        if (cardSystem == null || reward.CardData == null)
        {
            rewardModalView?.ShowStatus("Система карт не назначена");
            NotifyEffect(EffectType.GiveCard, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning("Cannot claim card reward. CardSystem or CardData is missing.");
            return false;
        }

        if (cardSystem.Hand.Count >= cardSystem.MaxCards)
        {
            rewardModalView?.ShowStatus("Рука карт заполнена");
            NotifyEffect(EffectType.GiveCard, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Card reward '{reward.DisplayName}' was not claimed. Card hand is full.");
            return false;
        }

        if (!cardSystem.AddCard(reward.CardData))
        {
            rewardModalView?.ShowStatus("Рука карт заполнена");
            NotifyEffect(EffectType.GiveCard, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Card reward '{reward.DisplayName}' was not claimed.");
            return false;
        }

        Debug.Log($"Card reward claimed: {reward.DisplayName}.");
        NotifyEffect(EffectType.GiveCard, 1, EffectNotificationStatus.Success);
        return true;
    }

    private bool TryClaimItemReward(RewardData reward)
    {
        if (playerInventory == null || reward.ItemData == null)
        {
            rewardModalView?.ShowStatus("Инвентарь экипировки не назначен");
            NotifyEffect(EffectType.GiveItem, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning("Cannot claim item reward. PlayerInventory or ItemData is missing.");
            return false;
        }

        if (!playerInventory.HasFreeSlot())
        {
            rewardModalView?.ShowStatus("Инвентарь экипировки заполнен");
            NotifyEffect(EffectType.GiveItem, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Item reward '{reward.DisplayName}' was not claimed. Equipment inventory is full.");
            return false;
        }

        if (!playerInventory.TryEquip(reward.ItemData))
        {
            rewardModalView?.ShowStatus("Инвентарь экипировки заполнен");
            NotifyEffect(EffectType.GiveItem, 1, EffectNotificationStatus.Failed);
            Debug.LogWarning($"Item reward '{reward.DisplayName}' was not claimed.");
            return false;
        }

        Debug.Log($"Item reward claimed: {reward.DisplayName}.");
        NotifyEffect(EffectType.GiveItem, 1, EffectNotificationStatus.Success);
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
