using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Окно выбора награды после победы в бою
/// </summary>

public sealed class RewardModalView : MonoBehaviour
{
    [SerializeField] private RewardView[] rewardViews;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text statusText;
    [SerializeField] private CanvasGroup statusCanvasGroup;
    [SerializeField, Min(0f)] private float statusFadeDuration = 0.2f;
    [SerializeField, Min(0f)] private float statusVisibleDuration = 1.2f;

    private Coroutine statusFade;

    public event Action<RewardData> RewardSelected;
    public event Action CloseRequested;
    /// <summary>
    /// Показывает окно выбора награды после боя
    /// </summary>
    public void Show(IReadOnlyList<RewardData> rewards)
    {
        gameObject.SetActive(true);
        HideStatus(true);
        Refresh(rewards);
    }
    /// <summary>
    /// Закрывает окно выбора награды
    /// </summary>
    public void Hide()
    {
        HideStatus(true);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Показывает подсказку внутри окна выбора награды
    /// </summary>
    public void ShowStatus(string message)
    {
        if (statusText == null)
            return;

        StopStatusFade();
        statusText.text = message ?? string.Empty;

        if (statusCanvasGroup != null)
            statusCanvasGroup.gameObject.SetActive(true);
        else
            statusText.gameObject.SetActive(true);

        statusFade = StartCoroutine(FadeStatusRoutine());
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
        HideStatus(true);
    }
    /// <summary>
    /// Подписывается на события другой системы
    /// </summary>
    private void Subscribe()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView == null)
                continue;

            rewardView.Clicked -= HandleRewardClicked;
            rewardView.Clicked += HandleRewardClicked;
        }
    }
    /// <summary>
    /// Отписывается от событий другой системы
    /// </summary>
    private void Unsubscribe()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);

        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView != null)
                rewardView.Clicked -= HandleRewardClicked;
        }
    }
    /// <summary>
    /// Заполняет окно выбора награды доступными вариантами
    /// </summary>
    private void Refresh(IReadOnlyList<RewardData> rewards)
    {
        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView == null)
                continue;

            var reward = rewards != null && i < rewards.Count ? rewards[i] : null;
            rewardView.SetReward(reward);
            rewardView.gameObject.SetActive(reward != null);
        }
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleRewardClicked(RewardData reward)
    {
        RewardSelected?.Invoke(reward);
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
    /// <summary>
    /// Показывает подсказку награды на короткое время
    /// </summary>
    private IEnumerator FadeStatusRoutine()
    {
        yield return FadeStatusTo(1f);

        if (statusVisibleDuration > 0f)
            yield return new WaitForSecondsRealtime(statusVisibleDuration);

        yield return FadeStatusTo(0f);
        statusFade = null;
    }
    /// <summary>
    /// Плавно меняет прозрачность подсказки награды
    /// </summary>
    private IEnumerator FadeStatusTo(float targetAlpha)
    {
        var startAlpha = GetStatusAlpha();

        if (statusFadeDuration <= 0f)
        {
            SetStatusAlpha(targetAlpha);
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < statusFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / statusFadeDuration);
            SetStatusAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
            yield return null;
        }

        SetStatusAlpha(targetAlpha);
    }
    /// <summary>
    /// Скрывает подсказку в окне выбора награды
    /// </summary>
    private void HideStatus(bool instant)
    {
        StopStatusFade();

        if (statusText != null)
            statusText.text = string.Empty;

        if (instant)
            SetStatusAlpha(0f);
    }
    /// <summary>
    /// Возвращает текущую прозрачность подсказки награды
    /// </summary>
    private float GetStatusAlpha()
    {
        if (statusCanvasGroup != null)
            return statusCanvasGroup.alpha;

        return statusText != null ? statusText.color.a : 0f;
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    private void SetStatusAlpha(float alpha)
    {
        if (statusCanvasGroup != null)
        {
            statusCanvasGroup.alpha = alpha;
            return;
        }

        if (statusText == null)
            return;

        var color = statusText.color;
        color.a = alpha;
        statusText.color = color;
    }
    /// <summary>
    /// Останавливает текущий процесс или анимацию
    /// </summary>
    private void StopStatusFade()
    {
        if (statusFade == null)
            return;

        StopCoroutine(statusFade);
        statusFade = null;
    }
}
