using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с InventorySlotView
/// </summary>

public sealed class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite emptyBackgroundSprite;
    [SerializeField] private Sprite occupiedBackgroundSprite;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button removeButton;
    [SerializeField] private CanvasGroup removeButtonCanvasGroup;
    [SerializeField, Min(0f)] private float removeButtonFadeDuration = 0.15f;
    [SerializeField] private ItemData item;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color occupiedColor = Color.white;

    private Coroutine removeButtonFade;
    private bool isPointerOver;

    public event Action<ItemData> RemoveClicked;

    public bool IsOccupied => item != null || itemIcon != null;
    public ItemData Item => item;
    public Sprite ItemIcon => item != null ? item.ItemSprite : itemIcon;
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClicked);

        Refresh();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (removeButton != null)
            removeButton.onClick.RemoveListener(HandleRemoveClicked);

        StopRemoveButtonFade();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        Refresh();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetItem(ItemData newItem)
    {
        item = newItem;
        itemIcon = null;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetItemIcon(Sprite newItemIcon)
    {
        item = null;
        itemIcon = newItemIcon;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    public void Clear()
    {
        item = null;
        itemIcon = null;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    public void Refresh()
    {
        if (backgroundImage != null)
        {
            var backgroundSprite = IsOccupied ? occupiedBackgroundSprite : emptyBackgroundSprite;
            if (backgroundSprite != null)
                backgroundImage.sprite = backgroundSprite;

            backgroundImage.color = IsOccupied ? occupiedColor : emptyColor;
        }

        if (removeButton != null)
            removeButton.gameObject.SetActive(item != null);

        SetRemoveButtonVisible(item != null && isPointerOver, true);

        if (iconImage == null)
            return;

        iconImage.sprite = ItemIcon;
        iconImage.enabled = ItemIcon != null;
        iconImage.color = Color.white;
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleRemoveClicked()
    {
        if (item != null)
            RemoveClicked?.Invoke(item);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        SetRemoveButtonVisible(item != null, false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        SetRemoveButtonVisible(false, false);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private void SetRemoveButtonVisible(bool visible, bool instant)
    {
        if (removeButtonCanvasGroup == null)
            return;

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
    /// Выполняет вспомогательную часть логики метода FadeRemoveButton
    /// </summary>
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
    /// Останавливает текущий процесс, корутину или визуальный переход
    /// </summary>
    private void StopRemoveButtonFade()
    {
        if (removeButtonFade == null)
            return;

        StopCoroutine(removeButtonFade);
        removeButtonFade = null;
    }
}
