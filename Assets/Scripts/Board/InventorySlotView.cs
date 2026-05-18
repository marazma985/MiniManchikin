using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button removeButton;
    [SerializeField] private ItemData item;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private Color emptyColor = new Color(0.22f, 0.2f, 0.35f, 0.9f);
    [SerializeField] private Color occupiedColor = Color.white;

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
}
