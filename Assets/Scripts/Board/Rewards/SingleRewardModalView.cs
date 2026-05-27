using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за выдачу или показ наград, связанные с SingleRewardModalView
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
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void Show(RewardData reward)
    {
        gameObject.SetActive(true);
        SetReward(reward);
    }
    /// <summary>
    /// Скрывает нужное окно или визуальное состояние
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
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
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetAcceptState(bool canAccept, string status)
    {
        if (acceptButton != null)
            acceptButton.interactable = canAccept;

        if (statusText != null)
            statusText.text = status ?? string.Empty;
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(HandleCloseClicked);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleAcceptClicked()
    {
        AcceptRequested?.Invoke();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
}
