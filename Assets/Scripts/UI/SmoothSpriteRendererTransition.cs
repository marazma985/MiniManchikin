using System.Collections;
using UnityEngine;
/// <summary>
/// Плавно меняет картинку кнопки, которая сделана через SpriteRenderer
/// </summary>

public sealed class SmoothSpriteRendererTransition : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private SpriteRenderer transitionRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField, Min(0f)] private float fadeDuration = 0.12f;

    private Coroutine spriteFade;
    private bool isPointerOver;
    private bool isPointerPressed;
    /// <summary>
    /// Автоматически находит SpriteRenderer для плавной смены спрайтов
    /// </summary>
    private void Reset()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
    }
    /// <summary>
    /// Показывает стартовый спрайт SpriteRenderer без плавного перехода
    /// </summary>
    private void OnEnable()
    {
        ConfigureRenderers();
        RefreshVisualState(true);
    }
    /// <summary>
    /// Останавливает плавную смену спрайта при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        StopTransition();
        isPointerOver = false;
        isPointerPressed = false;
    }
    /// <summary>
    /// Подключает SpriteRenderer, его спрайты и время плавной смены состояния
    /// </summary>
    public void Configure(SpriteRenderer sourceRenderer, SpriteRenderer sourceTransitionRenderer, Sprite normal, Sprite highlighted, Sprite pressed, float duration)
    {
        mainRenderer = sourceRenderer != null ? sourceRenderer : GetComponent<SpriteRenderer>();
        transitionRenderer = sourceTransitionRenderer;
        normalSprite = normal;
        highlightedSprite = highlighted;
        pressedSprite = pressed;
        fadeDuration = Mathf.Max(0f, duration);

        ConfigureRenderers();
        RefreshVisualState(true);
    }
    /// <summary>
    /// Передает SpriteRenderer состояние наведения и нажатия
    /// </summary>
    public void SetInteractionState(bool pointerOver, bool pointerPressed, bool instant)
    {
        isPointerOver = pointerOver;
        isPointerPressed = pointerPressed;
        RefreshVisualState(instant);
    }
    /// <summary>
    /// Останавливает плавную смену спрайта SpriteRenderer
    /// </summary>
    public void StopTransition()
    {
        if (spriteFade != null)
        {
            StopCoroutine(spriteFade);
            spriteFade = null;
        }

        if (transitionRenderer != null)
        {
            transitionRenderer.sprite = null;
            transitionRenderer.enabled = false;
        }

        if (mainRenderer != null)
            mainRenderer.color = Color.white;
    }
    /// <summary>
    /// Создает второй SpriteRenderer для плавного исчезновения прошлого спрайта
    /// </summary>
    private void ConfigureRenderers()
    {
        if (mainRenderer != null)
            mainRenderer.color = Color.white;

        if (transitionRenderer == null)
            return;

        transitionRenderer.sprite = null;
        transitionRenderer.enabled = false;

        if (mainRenderer != null)
        {
            transitionRenderer.sortingLayerID = mainRenderer.sortingLayerID;
            transitionRenderer.sortingOrder = mainRenderer.sortingOrder + 1;
        }
    }
    /// <summary>
    /// Обновляет спрайт объекта с учетом наведения, нажатия и блокировки
    /// </summary>
    private void RefreshVisualState(bool instant)
    {
        SetSprite(GetStateSprite(), instant);
    }
    /// <summary>
    /// Выбирает спрайт для текущего состояния SpriteRenderer
    /// </summary>
    private Sprite GetStateSprite()
    {
        if (isPointerPressed)
            return pressedSprite != null ? pressedSprite : GetHighlightedSprite();

        if (isPointerOver)
            return GetHighlightedSprite();

        return normalSprite;
    }
    /// <summary>
    /// Возвращает hover-спрайт или обычный спрайт, если отдельного нет
    /// </summary>
    private Sprite GetHighlightedSprite()
    {
        return highlightedSprite != null ? highlightedSprite : normalSprite;
    }
    /// <summary>
    /// Меняет спрайт SpriteRenderer сразу или с плавным переходом
    /// </summary>
    private void SetSprite(Sprite targetSprite, bool instant)
    {
        if (mainRenderer == null || targetSprite == null)
            return;

        if (mainRenderer.sprite == targetSprite)
        {
            if (instant)
                StopTransition();

            return;
        }

        var previousSprite = mainRenderer.sprite;
        mainRenderer.sprite = targetSprite;
        mainRenderer.color = Color.white;

        if (instant || previousSprite == null || transitionRenderer == null || fadeDuration <= 0f)
        {
            StopTransition();
            return;
        }

        if (spriteFade != null)
            StopCoroutine(spriteFade);

        transitionRenderer.sprite = previousSprite;
        transitionRenderer.color = Color.white;
        transitionRenderer.enabled = true;
        spriteFade = StartCoroutine(FadePreviousSprite());
    }
    /// <summary>
    /// Плавно скрывает прошлый спрайт SpriteRenderer
    /// </summary>
    private IEnumerator FadePreviousSprite()
    {
        var elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / fadeDuration);
            transitionRenderer.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }

        if (transitionRenderer != null)
        {
            transitionRenderer.sprite = null;
            transitionRenderer.enabled = false;
        }

        spriteFade = null;
    }
}
