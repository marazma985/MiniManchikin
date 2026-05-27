using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Отвечает за часть игровой логики или интерфейса, связанную с SmoothSpriteButton
/// </summary>

public sealed class SmoothSpriteButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField, Min(0f)] private float fadeDuration = 0.12f;

    private Image transitionImage;
    private Coroutine spriteFade;
    private bool isPointerOver;
    private bool isPointerPressed;
    private bool isSelected;
    private bool wasInteractable;
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        targetImage = GetComponent<Image>();
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        ConfigureButton();
        RefreshVisualState(true);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        StopTransition();
        isPointerOver = false;
        isPointerPressed = false;
        isSelected = false;
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        ResolveReferences();
    }
    /// <summary>
    /// Обновляет визуальное состояние в конце кадра после работы остальных систем
    /// </summary>
    private void LateUpdate()
    {
        if (button == null || wasInteractable == button.interactable)
            return;

        RefreshVisualState(false);
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(Button sourceButton, Image sourceImage, Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled, float duration)
    {
        button = sourceButton != null ? sourceButton : GetComponent<Button>();
        targetImage = sourceImage != null ? sourceImage : GetComponent<Image>();
        normalSprite = normal;
        highlightedSprite = highlighted;
        pressedSprite = pressed;
        disabledSprite = disabled;
        fadeDuration = Mathf.Max(0f, duration);

        ConfigureButton();
        RefreshVisualState(true);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetInteractionState(bool pointerOver, bool pointerPressed, bool selected, bool instant)
    {
        isPointerOver = pointerOver;
        isPointerPressed = pointerPressed;
        isSelected = selected;
        RefreshVisualState(instant);
    }
    /// <summary>
    /// Останавливает текущий процесс, корутину или визуальный переход
    /// </summary>
    public void StopTransition()
    {
        if (spriteFade != null)
        {
            StopCoroutine(spriteFade);
            spriteFade = null;
        }

        if (transitionImage != null)
        {
            transitionImage.sprite = null;
            transitionImage.enabled = false;
        }

        if (targetImage != null)
            targetImage.color = Color.white;
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
            return;

        isPointerPressed = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerPressed = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на событие select
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на событие deselect
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
    /// </summary>
    private void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    private void ConfigureButton()
    {
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            wasInteractable = button.interactable;
        }

        if (targetImage != null)
            targetImage.color = Color.white;
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private void RefreshVisualState(bool instant)
    {
        if (targetImage == null)
            return;

        if (button != null)
            wasInteractable = button.interactable;

        SetButtonSprite(GetStateSprite(), instant);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private Sprite GetStateSprite()
    {
        if (button != null && !button.interactable)
            return disabledSprite != null ? disabledSprite : normalSprite;

        if (isPointerPressed)
            return pressedSprite != null ? pressedSprite : GetHighlightedSprite();

        if (isPointerOver || isSelected)
            return GetHighlightedSprite();

        return normalSprite;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private Sprite GetHighlightedSprite()
    {
        return highlightedSprite != null ? highlightedSprite : normalSprite;
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private void SetButtonSprite(Sprite targetSprite, bool instant)
    {
        if (targetSprite == null)
            return;

        if (targetImage.sprite == targetSprite)
        {
            if (instant)
                StopTransition();

            return;
        }

        var previousSprite = targetImage.sprite;
        targetImage.sprite = targetSprite;
        targetImage.color = Color.white;

        if (instant || previousSprite == null || fadeDuration <= 0f)
        {
            StopTransition();
            return;
        }

        EnsureTransitionImage();
        if (transitionImage == null)
            return;

        if (spriteFade != null)
            StopCoroutine(spriteFade);

        transitionImage.sprite = previousSprite;
        transitionImage.color = Color.white;
        transitionImage.enabled = true;
        spriteFade = StartCoroutine(FadePreviousSprite());
    }
    /// <summary>
    /// Гарантирует, что нужный объект, ресурс или ссылка существует
    /// </summary>
    private void EnsureTransitionImage()
    {
        if (transitionImage != null || targetImage == null)
            return;

        var transitionObject = new GameObject("Smooth Sprite Transition", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var transitionTransform = transitionObject.GetComponent<RectTransform>();
        transitionTransform.SetParent(targetImage.rectTransform, false);
        transitionTransform.anchorMin = Vector2.zero;
        transitionTransform.anchorMax = Vector2.one;
        transitionTransform.pivot = targetImage.rectTransform.pivot;
        transitionTransform.anchoredPosition = Vector2.zero;
        transitionTransform.sizeDelta = Vector2.zero;
        transitionTransform.localScale = Vector3.one;
        transitionTransform.localRotation = Quaternion.identity;
        transitionTransform.SetAsLastSibling();

        transitionImage = transitionObject.GetComponent<Image>();
        transitionImage.raycastTarget = false;
        transitionImage.type = targetImage.type;
        transitionImage.preserveAspect = targetImage.preserveAspect;
        transitionImage.pixelsPerUnitMultiplier = targetImage.pixelsPerUnitMultiplier;
        transitionImage.enabled = false;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода FadePreviousSprite
    /// </summary>
    private IEnumerator FadePreviousSprite()
    {
        var elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / fadeDuration);
            transitionImage.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }

        if (transitionImage != null)
        {
            transitionImage.sprite = null;
            transitionImage.enabled = false;
        }

        spriteFade = null;
    }
}
