using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Один слот предмета в интерфейсе, где показывается экипировка и кнопка снятия предмета
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
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClicked);

        Refresh();
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (removeButton != null)
            removeButton.onClick.RemoveListener(HandleRemoveClicked);

        StopRemoveButtonFade();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        Refresh();
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetItem(ItemData newItem)
    {
        item = newItem;
        itemIcon = null;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetItemIcon(Sprite newItemIcon)
    {
        item = null;
        itemIcon = newItemIcon;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Очищает ячейку экипировки, когда в ней нет предмета
    /// </summary>
    public void Clear()
    {
        item = null;
        itemIcon = null;
        isPointerOver = false;
        Refresh();
    }
    /// <summary>
    /// Обновляет одну ячейку экипировки и ее кнопку снятия
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
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleRemoveClicked()
    {
        if (item != null)
            RemoveClicked?.Invoke(item);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        SetRemoveButtonVisible(item != null, false);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        SetRemoveButtonVisible(false, false);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
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
    /// Плавно показывает или скрывает крестик снятия предмета
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
    /// Останавливает текущий процесс или анимацию
    /// </summary>
    private void StopRemoveButtonFade()
    {
        if (removeButtonFade == null)
            return;

        StopCoroutine(removeButtonFade);
        removeButtonFade = null;
    }
}
