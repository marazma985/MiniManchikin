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

    public event Action RewardStateChanged;

    public RewardData CurrentReward => currentReward;

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
        RewardStateChanged?.Invoke();
        return true;
    }

    public bool RestoreReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
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
        return currentReward != null && currentReward.CanClaim(cardSystem, playerInventory, out status);
    }

    private bool TryClaimCurrentReward()
    {
        return currentReward != null && currentReward.TryClaim(cardSystem, playerInventory);
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
        RewardStateChanged?.Invoke();
    }
}
