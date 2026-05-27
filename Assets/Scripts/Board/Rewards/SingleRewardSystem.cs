using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Показывает одиночную награду и проверяет, может ли игрок ее забрать
/// </summary>

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
    /// <summary>
    /// Открывает окно одиночной награды и ждет подтверждения игрока
    /// </summary>
    public bool ShowReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
    }
    /// <summary>
    /// Открывает окно одиночной награды и ждет подтверждения игрока
    /// </summary>
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
    /// <summary>
    /// Восстанавливает открытое окно одиночной награды из сохранения
    /// </summary>
    public bool RestoreReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
    }
    /// <summary>
    /// Подписывает одиночную награду на окно и изменения инвентаря
    /// </summary>
    private void OnEnable()
    {
        SubscribeModal();
        SubscribeAvailability();
    }
    /// <summary>
    /// Отписывает одиночную награду от окна и изменений инвентаря
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeModal();
        UnsubscribeAvailability();
    }
    /// <summary>
    /// Подписывает систему одиночной награды на кнопки ее окна
    /// </summary>
    private void SubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested += HandleAcceptRequested;
        modalView.CloseRequested += HandleCloseRequested;
    }
    /// <summary>
    /// Отписывает систему одиночной награды от кнопок ее окна
    /// </summary>
    private void UnsubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested -= HandleAcceptRequested;
        modalView.CloseRequested -= HandleCloseRequested;
    }
    /// <summary>
    /// Подписывается на руку и экипировку, чтобы обновлять доступность награды
    /// </summary>
    private void SubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged += HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged += HandleEquipmentChanged;
    }
    /// <summary>
    /// Отписывается от руки и экипировки после закрытия награды
    /// </summary>
    private void UnsubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged -= HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged -= HandleEquipmentChanged;
    }
    /// <summary>
    /// Пытается выдать одиночную награду после нажатия кнопки
    /// </summary>
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
    /// <summary>
    /// Закрывает окно одиночной награды после завершения выдачи
    /// </summary>
    private void HandleCloseRequested()
    {
        Debug.Log("Single reward declined.");
        CompleteReward();
    }
    /// <summary>
    /// Обновляет кнопку принятия награды после изменения руки карт
    /// </summary>
    private void HandleHandChanged(IReadOnlyList<CardData> cards)
    {
        RefreshAcceptState();
    }
    /// <summary>
    /// Обновляет кнопку принятия награды после изменения экипировки
    /// </summary>
    private void HandleEquipmentChanged(IReadOnlyList<ItemData> items)
    {
        RefreshAcceptState();
    }
    /// <summary>
    /// Включает кнопку принятия одиночной награды, когда ее можно забрать
    /// </summary>
    private void RefreshAcceptState()
    {
        if (modalView == null || currentReward == null)
            return;

        var canAccept = CanAcceptCurrentReward(out var status);
        modalView.SetAcceptState(canAccept, status);
    }
    /// <summary>
    /// Проверяет, можно ли сейчас выполнить это действие
    /// </summary>
    private bool CanAcceptCurrentReward(out string status)
    {
        status = string.Empty;
        return currentReward != null && currentReward.CanClaim(cardSystem, playerInventory, out status);
    }
    /// <summary>
    /// Пытается выдать текущую одиночную награду игроку
    /// </summary>
    private bool TryClaimCurrentReward()
    {
        return currentReward != null && currentReward.TryClaim(cardSystem, playerInventory);
    }
    /// <summary>
    /// Выдает одиночную награду игроку и закрывает окно
    /// </summary>
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
