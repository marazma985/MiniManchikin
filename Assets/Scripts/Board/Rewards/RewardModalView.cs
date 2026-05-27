using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за выдачу или показ наград, связанные с RewardModalView
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
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void Show(IReadOnlyList<RewardData> rewards)
    {
        gameObject.SetActive(true);
        HideStatus(true);
        Refresh(rewards);
    }
    /// <summary>
    /// Скрывает нужное окно или визуальное состояние
    /// </summary>
    public void Hide()
    {
        HideStatus(true);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
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
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
        HideStatus(true);
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
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
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
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
    /// Обновляет отображение на основе текущих данных
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
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleRewardClicked(RewardData reward)
    {
        RewardSelected?.Invoke(reward);
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода FadeStatusRoutine
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
    /// Выполняет вспомогательную часть логики метода FadeStatusTo
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
    /// Скрывает нужное окно или визуальное состояние
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
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private float GetStatusAlpha()
    {
        if (statusCanvasGroup != null)
            return statusCanvasGroup.alpha;

        return statusText != null ? statusText.color.a : 0f;
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
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
    /// Останавливает текущий процесс, корутину или визуальный переход
    /// </summary>
    private void StopStatusFade()
    {
        if (statusFade == null)
            return;

        StopCoroutine(statusFade);
        statusFade = null;
    }
}
