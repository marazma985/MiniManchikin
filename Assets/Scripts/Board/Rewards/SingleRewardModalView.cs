using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Окно с одной наградой, которое открывается после некоторых клеток поля
/// </summary>

public sealed class SingleRewardModalView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text statusText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;

    public event Action AcceptRequested;
    public event Action CloseRequested;
    /// <summary>
    /// Показывает окно с одной полученной наградой
    /// </summary>
    public void Show(RewardData reward)
    {
        gameObject.SetActive(true);
        SetReward(reward);
    }
    /// <summary>
    /// Закрывает окно одиночной награды
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetReward(RewardData reward)
    {
        if (nameText != null)
            nameText.text = reward != null ? reward.DisplayName : string.Empty;

        if (descriptionText != null)
            descriptionText.text = reward != null ? reward.DisplayDescription : string.Empty;

        if (iconImage != null)
        {
            var sprite = reward != null ? reward.DisplaySprite : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetAcceptState(bool canAccept, string status)
    {
        if (acceptButton != null)
            acceptButton.interactable = canAccept;

        if (statusText != null)
            statusText.text = status ?? string.Empty;
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(HandleCloseClicked);
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleAcceptClicked()
    {
        AcceptRequested?.Invoke();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
}
