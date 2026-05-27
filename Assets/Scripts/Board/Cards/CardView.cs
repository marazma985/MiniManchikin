using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Отдельная карта на экране, которую игрок видит в руке и может нажать
/// </summary>

public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Image hoverOverlayImage;
    [SerializeField] private Button button;
    [SerializeField] private Button removeButton;
    [SerializeField] private CanvasGroup removeButtonCanvasGroup;
    [SerializeField, Min(0f)] private float removeButtonFadeDuration = 0.15f;
    [SerializeField, Min(0f)] private float cardHoverFadeDuration = 0.12f;
    [SerializeField, Range(0f, 1f)] private float cardHoverOverlayAlpha = 0.18f;

    private CardData currentCard;
    private Coroutine removeButtonFade;
    private Coroutine cardHoverFade;
    private bool isPointerOver;
    private bool isRemoveButtonVisible;
    private bool isCardHighlighted;
    private RectTransform cardRectTransform;
    private Canvas parentCanvas;
    private bool buttonVisualConfigured;

    public event Action<CardData> Clicked;
    public event Action<CardData> RemoveClicked;

    /// <summary>
    /// Карта, которую сейчас показывает этот UI-слот
    /// </summary>
    public CardData CurrentCard => currentCard;
    /// <summary>
    /// Подписывает карту на клики и готовит крестик удаления
    /// </summary>
    private void OnEnable()
    {
        ConfigureButtonVisuals();

        if (button != null)
            button.onClick.AddListener(HandleClick);

        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClick);

        RefreshVisual();
    }
    /// <summary>
    /// Отписывает карту от кликов и сбрасывает подсветку
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        if (removeButton != null)
            removeButton.onClick.RemoveListener(HandleRemoveClick);

        StopRemoveButtonFade();
        StopCardHoverFade();
        SetCardHover(false, true);
    }
    /// <summary>
    /// Подставляет карту в UI-слот и сразу обновляет ее вид
    /// </summary>
    /// <param name="card">Карта, которую должен показать этот UI-слот</param>
    public void SetCard(CardData card)
    {
        currentCard = card;
        isPointerOver = currentCard != null && IsPointerOverCard(Input.mousePosition);
        RefreshVisual();
    }
    /// <summary>
    /// Обновляет картинку, текст и кнопку удаления на одной карте
    /// </summary>
    private void RefreshVisual()
    {
        if (removeButton != null)
            removeButton.gameObject.SetActive(currentCard != null);

        var hasCard = currentCard != null;
        SetRemoveButtonVisible(hasCard && isPointerOver, true);

        if (cardImage == null)
            return;

        var sprite = hasCard ? currentCard.CardSprite : null;
        cardImage.sprite = sprite;
        cardImage.enabled = sprite != null;

        RefreshHoverOverlaySprite(sprite);
        SetCardHover(hasCard && isPointerOver, true);
    }
    /// <summary>
    /// Сообщает, что игрок нажал на эту карту
    /// </summary>
    private void HandleClick()
    {
        if (currentCard != null)
            Clicked?.Invoke(currentCard);
    }
    /// <summary>
    /// Сообщает, что игрок нажал крестик удаления этой карты
    /// </summary>
    private void HandleRemoveClick()
    {
        if (currentCard != null)
            RemoveClicked?.Invoke(currentCard);
    }
    /// <summary>
    /// Показывает подсветку и крестик, когда мышь входит в область карты
    /// </summary>
    /// <param name="eventData">Данные события наведения от Unity</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = currentCard != null;
        SetRemoveButtonVisible(isPointerOver, false);
        SetCardHover(isPointerOver, false);
    }
    /// <summary>
    /// Скрывает подсветку и крестик, когда мышь уходит с карты
    /// </summary>
    /// <param name="eventData">Данные события наведения от Unity</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        SetCardHover(false, false);
        SetRemoveButtonVisible(false, false);
    }
    /// <summary>
    /// Настраивает крестик удаления карты и его плавную смену спрайтов
    /// </summary>
    private void ConfigureButtonVisuals()
    {
        if (button != null && !buttonVisualConfigured)
        {
            EnsureHoverOverlay();
            button.transition = Selectable.Transition.None;
            SetCardHover(false, true);
            buttonVisualConfigured = true;
        }
    }
    /// <summary>
    /// Проверяет, наведена ли мышь именно на эту карту
    /// </summary>
    /// <param name="screenPosition">Позиция мыши на экране</param>
    private bool IsPointerOverCard(Vector2 screenPosition)
    {
        if (cardRectTransform == null)
            cardRectTransform = transform as RectTransform;

        return cardRectTransform != null &&
               RectTransformUtility.RectangleContainsScreenPoint(cardRectTransform, screenPosition, GetEventCamera());
    }
    /// <summary>
    /// Возвращает камеру, через которую UI проверяет наведение на карту
    /// </summary>
    private Camera GetEventCamera()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return parentCanvas.worldCamera;
    }
    /// <summary>
    /// Включает или выключает подсветку карты при наведении
    /// </summary>
    /// <param name="highlighted">Должна ли карта быть подсвечена</param>
    /// <param name="instant">Нужно ли применить изменение сразу, без анимации</param>
    private void SetCardHover(bool highlighted, bool instant)
    {
        if (hoverOverlayImage == null)
            return;

        if (!instant && isCardHighlighted == highlighted)
            return;

        isCardHighlighted = highlighted;
        var targetAlpha = highlighted ? cardHoverOverlayAlpha : 0f;

        StopCardHoverFade();
        if (instant || cardHoverFadeDuration <= 0f)
        {
            SetHoverOverlayAlpha(targetAlpha);
            return;
        }

        cardHoverFade = StartCoroutine(FadeCardHover(targetAlpha));
    }
    /// <summary>
    /// Плавно показывает или скрывает подсветку карты при наведении
    /// </summary>
    /// <param name="targetAlpha">Прозрачность подсветки, к которой идет анимация</param>
    private IEnumerator FadeCardHover(float targetAlpha)
    {
        var startAlpha = hoverOverlayImage.color.a;
        var elapsed = 0f;

        while (elapsed < cardHoverFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / cardHoverFadeDuration);
            SetHoverOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
            yield return null;
        }

        SetHoverOverlayAlpha(targetAlpha);
        cardHoverFade = null;
    }
    /// <summary>
    /// Создает слой подсветки поверх карты, если он еще не назначен
    /// </summary>
    private void EnsureHoverOverlay()
    {
        if (hoverOverlayImage != null)
        {
            hoverOverlayImage.raycastTarget = false;
            SetHoverOverlayAlpha(0f);
            return;
        }

        if (cardImage == null)
            return;

        var overlayObject = new GameObject("Hover Light Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayObject.transform.SetParent(transform, false);
        overlayObject.transform.SetAsFirstSibling();

        if (overlayObject.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        hoverOverlayImage = overlayObject.GetComponent<Image>();
        hoverOverlayImage.raycastTarget = false;
        RefreshHoverOverlaySprite(cardImage.sprite);
        SetHoverOverlayAlpha(0f);
    }
    /// <summary>
    /// Подставляет спрайт подсветки поверх карты
    /// </summary>
    /// <param name="sprite">Спрайт, который нужно показать</param>
    private void RefreshHoverOverlaySprite(Sprite sprite)
    {
        if (hoverOverlayImage == null)
            return;

        hoverOverlayImage.sprite = sprite;
        hoverOverlayImage.type = cardImage != null ? cardImage.type : Image.Type.Simple;
        hoverOverlayImage.preserveAspect = cardImage != null && cardImage.preserveAspect;
        hoverOverlayImage.enabled = sprite != null;
    }
    /// <summary>
    /// Меняет прозрачность подсветки карты через альфа-канал
    /// </summary>
    /// <param name="alpha">Новая прозрачность через альфа-канал</param>
    private void SetHoverOverlayAlpha(float alpha)
    {
        if (hoverOverlayImage == null)
            return;

        hoverOverlayImage.color = new Color(1f, 1f, 1f, alpha);
    }
    /// <summary>
    /// Показывает или скрывает крестик удаления карты
    /// </summary>
    /// <param name="visible">Должен ли крестик удаления карты быть видимым</param>
    /// <param name="instant">Нужно ли применить изменение сразу, без анимации</param>
    private void SetRemoveButtonVisible(bool visible, bool instant)
    {
        if (removeButtonCanvasGroup == null)
            return;

        if (!instant && isRemoveButtonVisible == visible)
        {
            removeButtonCanvasGroup.interactable = visible;
            removeButtonCanvasGroup.blocksRaycasts = visible;
            return;
        }

        isRemoveButtonVisible = visible;
        StopRemoveButtonFade();
        removeButtonCanvasGroup.interactable = visible;
        removeButtonCanvasGroup.blocksRaycasts = visible;

        if (instant || removeButtonFadeDuration <= 0f)
        {
            removeButtonCanvasGroup.alpha = visible ? 1f : 0f;
            return;
        }

        removeButtonFade = StartCoroutine(FadeRemoveButton(visible ? 1f : 0f));
    }
    /// <summary>
    /// Плавно показывает или скрывает крестик удаления карты
    /// </summary>
    /// <param name="targetAlpha">Прозрачность крестика, к которой идет анимация</param>
    private IEnumerator FadeRemoveButton(float targetAlpha)
    {
        var startAlpha = removeButtonCanvasGroup.alpha;
        var elapsed = 0f;

        while (elapsed < removeButtonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / removeButtonFadeDuration);
            removeButtonCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        removeButtonCanvasGroup.alpha = targetAlpha;
        removeButtonFade = null;
    }
    /// <summary>
    /// Останавливает плавное появление или скрытие крестика карты
    /// </summary>
    private void StopRemoveButtonFade()
    {
        if (removeButtonFade == null)
            return;

        StopCoroutine(removeButtonFade);
        removeButtonFade = null;
    }
    /// <summary>
    /// Останавливает плавное появление или скрытие подсветки карты
    /// </summary>
    private void StopCardHoverFade()
    {
        if (cardHoverFade == null)
            return;

        StopCoroutine(cardHoverFade);
        cardHoverFade = null;
    }
}
