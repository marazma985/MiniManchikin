using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с HudView
/// </summary>

public sealed class HudView : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private InventorySlotView[] inventorySlots;

    /// <summary>
    /// Подписывается на системы игрока и обновляет весь HUD при включении
    /// </summary>
    private void OnEnable()
    {
        SubscribeSlots();
        Subscribe();
        RefreshAll();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
        UnsubscribeSlots();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetPlayerStats(PlayerStats newPlayerStats)
    {
        if (playerStats == newPlayerStats)
            return;

        Unsubscribe();
        playerStats = newPlayerStats;
        Subscribe();
        RefreshAll();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetPlayerInventory(PlayerInventory newPlayerInventory)
    {
        if (playerInventory == newPlayerInventory)
            return;

        UnsubscribeInventory();
        playerInventory = newPlayerInventory;
        SubscribeInventory();
        RefreshSlots();
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    public void RefreshAll()
    {
        if (playerStats == null)
        {
            UpdateLevel(0);
            UpdateHp(0, heartImages != null ? heartImages.Length : 0);
            RefreshSlots();
            return;
        }

        UpdateLevel(playerStats.Level);
        UpdateHp(playerStats.CurrentHp, playerStats.MaxHp);
        RefreshSlots();
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void Subscribe()
    {
        SubscribeStats();
        SubscribeInventory();
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void SubscribeStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged += UpdateHp;
        playerStats.OnLevelChanged += UpdateLevel;
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void Unsubscribe()
    {
        UnsubscribeStats();
        UnsubscribeInventory();
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void UnsubscribeStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= UpdateHp;
        playerStats.OnLevelChanged -= UpdateLevel;
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void SubscribeInventory()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnEquipmentChanged += UpdateEquipmentSlots;
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void UnsubscribeInventory()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnEquipmentChanged -= UpdateEquipmentSlots;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода UpdateHp
    /// </summary>
    private void UpdateHp(int currentHp, int maxHp)
    {
        if (heartImages == null)
            return;

        for (var i = 0; i < heartImages.Length; i++)
        {
            var heart = heartImages[i];
            if (heart == null)
                continue;

            var isFilled = i < currentHp;
            heart.enabled = i < maxHp;
            heart.sprite = isFilled ? fullHeartSprite : emptyHeartSprite;
            heart.color = isFilled ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода UpdateLevel
    /// </summary>
    private void UpdateLevel(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LVL {newLevel}";
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private void RefreshSlots()
    {
        UpdateEquipmentSlots(playerInventory != null ? playerInventory.GetEquippedItems() : null);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода UpdateEquipmentSlots
    /// </summary>
    private void UpdateEquipmentSlots(IReadOnlyList<ItemData> equippedItems)
    {
        if (inventorySlots == null)
            return;

        for (var i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null)
                continue;

            var item = equippedItems != null && i < equippedItems.Count ? equippedItems[i] : null;
            if (item != null)
                slot.SetItem(item);
            else
                slot.Clear();
        }
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void SubscribeSlots()
    {
        if (inventorySlots == null)
            return;

        for (var i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].RemoveClicked += HandleInventorySlotRemoveClicked;
        }
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void UnsubscribeSlots()
    {
        if (inventorySlots == null)
            return;

        for (var i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].RemoveClicked -= HandleInventorySlotRemoveClicked;
        }
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleInventorySlotRemoveClicked(ItemData item)
    {
        if (playerInventory != null)
            playerInventory.Unequip(item);
    }
}
