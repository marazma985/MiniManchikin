using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SingleRewardSystem : MonoBehaviour
{
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SingleRewardModalView modalView;

    private RewardData currentReward;
    private Action rewardCompleted;
    private Action<RewardData> rewardClaimed;

    public bool ShowReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
    }

    public bool ShowReward(RewardData reward, Action onCompleted, Action<RewardData> onClaimed)
    {
        if (modalView == null)
        {
            Debug.LogWarning("SingleRewardSystem requires SingleRewardModalView.");
            return false;
        }

        if (reward == null)
        {
            Debug.LogWarning("Cannot show missing single reward.");
            return false;
        }

        currentReward = reward;
        rewardCompleted = onCompleted;
        rewardClaimed = onClaimed;
        modalView.Show(reward);
        RefreshAcceptState();
        return true;
    }

    private void OnEnable()
    {
        SubscribeModal();
        SubscribeAvailability();
    }

    private void OnDisable()
    {
        UnsubscribeModal();
        UnsubscribeAvailability();
    }

    private void SubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested += HandleAcceptRequested;
        modalView.CloseRequested += HandleCloseRequested;
    }

    private void UnsubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested -= HandleAcceptRequested;
        modalView.CloseRequested -= HandleCloseRequested;
    }

    private void SubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged += HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void UnsubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged -= HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    private void HandleAcceptRequested()
    {
        if (!CanAcceptCurrentReward(out _))
        {
            RefreshAcceptState();
            return;
        }

        if (!TryClaimCurrentReward())
        {
            RefreshAcceptState();
            return;
        }

        rewardClaimed?.Invoke(currentReward);
        CompleteReward();
    }

    private void HandleCloseRequested()
    {
        Debug.Log("Single reward declined.");
        CompleteReward();
    }

    private void HandleHandChanged(IReadOnlyList<CardData> cards)
    {
        RefreshAcceptState();
    }

    private void HandleEquipmentChanged(IReadOnlyList<ItemData> items)
    {
        RefreshAcceptState();
    }

    private void RefreshAcceptState()
    {
        if (modalView == null || currentReward == null)
            return;

        var canAccept = CanAcceptCurrentReward(out var status);
        modalView.SetAcceptState(canAccept, status);
    }

    private bool CanAcceptCurrentReward(out string status)
    {
        status = string.Empty;
        if (currentReward == null)
            return false;

        switch (currentReward.RewardType)
        {
            case RewardType.Card:
                if (cardSystem == null)
                {
                    status = "Система карт не назначена";
                    return false;
                }

                if (cardSystem.Hand.Count >= cardSystem.MaxCards)
                {
                    status = "Рука карт заполнена";
                    return false;
                }

                return true;
            case RewardType.Item:
                if (playerInventory == null)
                {
                    status = "Инвентарь экипировки не назначен";
                    return false;
                }

                if (!playerInventory.HasFreeSlot())
                {
                    status = "Инвентарь экипировки заполнен";
                    return false;
                }

                return true;
            default:
                status = "Неподдерживаемая награда";
                return false;
        }
    }

    private bool TryClaimCurrentReward()
    {
        switch (currentReward.RewardType)
        {
            case RewardType.Card:
                return currentReward.CardData != null && cardSystem != null && cardSystem.AddCard(currentReward.CardData);
            case RewardType.Item:
                return currentReward.ItemData != null && playerInventory != null && playerInventory.TryEquip(currentReward.ItemData);
            default:
                return false;
        }
    }

    private void CompleteReward()
    {
        if (modalView != null)
            modalView.Hide();

        currentReward = null;

        var onCompleted = rewardCompleted;
        rewardCompleted = null;
        rewardClaimed = null;
        onCompleted?.Invoke();
    }
}
