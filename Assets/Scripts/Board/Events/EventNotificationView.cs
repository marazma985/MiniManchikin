using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Одна всплывающая подсказка на экране, которая появляется и затем плавно исчезает
/// </summary>

public sealed class EventNotificationView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text messageText;
    [SerializeField, Min(0f)] private float lifetime = 0.9f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;
    [SerializeField] private float moveUpDistance = 36f;

    private Coroutine lifecycleCoroutine;
    /// <summary>
    /// Показывает короткую подсказку события с текстом и иконкой
    /// </summary>
    public void Show(string message, Sprite icon)
    {
        if (messageText != null)
            messageText.text = message ?? string.Empty;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        StopLifecycle();
        lifecycleCoroutine = StartCoroutine(LifecycleRoutine());
    }
    /// <summary>
    /// Останавливает исчезновение всплывающей подсказки при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        StopLifecycle();
    }
    /// <summary>
    /// Держит подсказку на экране и затем плавно убирает ее
    /// </summary>
    private IEnumerator LifecycleRoutine()
    {
        if (lifetime > 0f)
            yield return new WaitForSecondsRealtime(lifetime);

        var startPosition = contentRoot != null ? contentRoot.anchoredPosition : Vector2.zero;
        var targetPosition = startPosition + new Vector2(0f, moveUpDistance);
        var elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - progress;

            if (contentRoot != null)
                contentRoot.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);

            yield return null;
        }

        Destroy(gameObject);
    }
    /// <summary>
    /// Останавливает жизненный цикл всплывающей подсказки
    /// </summary>
    private void StopLifecycle()
    {
        if (lifecycleCoroutine == null)
            return;

        StopCoroutine(lifecycleCoroutine);
        lifecycleCoroutine = null;
    }
}
