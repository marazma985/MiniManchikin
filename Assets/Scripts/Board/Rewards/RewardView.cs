using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за выдачу или показ наград, связанные с RewardView
/// </summary>

public sealed class RewardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button button;

    private RewardData currentReward;

    public event Action<RewardData> Clicked;
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetReward(RewardData reward)
    {
        currentReward = reward;
        Refresh();
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        Refresh();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private void Refresh()
    {
        if (nameText != null)
            nameText.text = currentReward != null ? currentReward.DisplayName : string.Empty;

        if (descriptionText != null)
            descriptionText.text = currentReward != null ? currentReward.DisplayDescription : string.Empty;

        if (iconImage != null)
        {
            var sprite = currentReward != null ? currentReward.DisplaySprite : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        if (button != null)
            button.interactable = currentReward != null;
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleClick()
    {
        if (currentReward != null)
            Clicked?.Invoke(currentReward);
    }
}
