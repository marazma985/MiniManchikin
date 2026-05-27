using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Одна карточка награды внутри окна выбора
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
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetReward(RewardData reward)
    {
        currentReward = reward;
        Refresh();
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        Refresh();
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
    /// <summary>
    /// Обновляет картинку и текст одной награды
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
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleClick()
    {
        if (currentReward != null)
            Clicked?.Invoke(currentReward);
    }
}
