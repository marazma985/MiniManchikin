using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button removeButton;
    [SerializeField] private CanvasGroup removeButtonCanvasGroup;
    [SerializeField, Min(0f)] private float removeButtonFadeDuration = 0.15f;
    [SerializeField] private ItemData item;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private Color emptyColor = new Color(0.22f, 0.2f, 0.35f, 0.9f);
    [SerializeField] private Color occupiedColor = Color.white;

    private Coroutine removeButtonFade;
    private bool isPointerOver;

    public event Action<ItemData> RemoveClicked;

    public bool IsOccupied => item != null || itemIcon != null;
    public ItemData Item => item;
    public Sprite ItemIcon => item != null ? item.ItemSprite : itemIcon;

    private void OnEnable()
    {
        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClicked);

        Refresh();
    }

    private void OnDisable()
    {
        if (removeButton != null)
            removeButton.onClick.RemoveListener(HandleRemoveClicked);

        StopRemoveButtonFade();
    }

    private void OnValidate()
    {
        Refresh();
    }

    public void SetItem(ItemData newItem)
    {
        item = newItem;
        itemIcon = null;
        Refresh();
    }

    public void SetItemIcon(Sprite newItemIcon)
    {
        item = null;
        itemIcon = newItemIcon;
        Refresh();
    }

    public void Clear()
    {
        item = null;
        itemIcon = null;
        Refresh();
    }

    public void Refresh()
    {
        if (backgroundImage != null)
            backgroundImage.color = IsOccupied ? occupiedColor : emptyColor;

        if (removeButton != null)
            removeButton.gameObject.SetActive(item != null);

        SetRemoveButtonVisible(item != null && isPointerOver, true);

        if (iconImage == null)
            return;

        iconImage.sprite = ItemIcon;
        iconImage.enabled = ItemIcon != null;
        iconImage.color = Color.white;
    }

    private void HandleRemoveClicked()
    {
        if (item != null)
            RemoveClicked?.Invoke(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        SetRemoveButtonVisible(item != null, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        SetRemoveButtonVisible(false, false);
    }

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

    private void StopRemoveButtonFade()
    {
        if (removeButtonFade == null)
            return;

        StopCoroutine(removeButtonFade);
        removeButtonFade = null;
    }
}
