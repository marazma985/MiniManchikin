using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Управляет получением одиночной награды с клетки поля и проверяет, может ли игрок ее принять
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
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public bool ShowReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
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
    /// Восстанавливает состояние из сохраненных данных
    /// </summary>
    public bool RestoreReward(RewardData reward, Action onCompleted)
    {
        return ShowReward(reward, onCompleted, null);
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        SubscribeModal();
        SubscribeAvailability();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeModal();
        UnsubscribeAvailability();
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void SubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested += HandleAcceptRequested;
        modalView.CloseRequested += HandleCloseRequested;
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void UnsubscribeModal()
    {
        if (modalView == null)
            return;

        modalView.AcceptRequested -= HandleAcceptRequested;
        modalView.CloseRequested -= HandleCloseRequested;
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void SubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged += HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged += HandleEquipmentChanged;
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void UnsubscribeAvailability()
    {
        if (cardSystem != null)
            cardSystem.OnHandChanged -= HandleHandChanged;

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged -= HandleEquipmentChanged;
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
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
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCloseRequested()
    {
        Debug.Log("Single reward declined.");
        CompleteReward();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleHandChanged(IReadOnlyList<CardData> cards)
    {
        RefreshAcceptState();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleEquipmentChanged(IReadOnlyList<ItemData> items)
    {
        RefreshAcceptState();
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private void RefreshAcceptState()
    {
        if (modalView == null || currentReward == null)
            return;

        var canAccept = CanAcceptCurrentReward(out var status);
        modalView.SetAcceptState(canAccept, status);
    }
    /// <summary>
    /// Проверяет, разрешено ли выполнить действие в текущем состоянии игры
    /// </summary>
    private bool CanAcceptCurrentReward(out string status)
    {
        status = string.Empty;
        return currentReward != null && currentReward.CanClaim(cardSystem, playerInventory, out status);
    }
    /// <summary>
    /// Пытается выполнить действие и возвращает, получилось ли это сделать
    /// </summary>
    private bool TryClaimCurrentReward()
    {
        return currentReward != null && currentReward.TryClaim(cardSystem, playerInventory);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода CompleteReward
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
