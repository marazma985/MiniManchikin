using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Обновляет интерфейс игрока на поле: сердца, уровень, кнопку кубика и слоты предметов
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
    /// Подписывает HUD на изменения игрока и сразу обновляет панели
    /// </summary>
    private void OnEnable()
    {
        SubscribeSlots();
        Subscribe();
        RefreshAll();
    }
    /// <summary>
    /// Отписывает HUD от событий игрока и ячеек экипировки
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
        UnsubscribeSlots();
    }
    /// <summary>
    /// Полностью обновляет HUD игрока: здоровье, уровень и предметы
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
    /// Подписывает HUD на изменения здоровья, уровня, экипировки и слотов
    /// </summary>
    private void Subscribe()
    {
        SubscribeStats();
        SubscribeInventory();
    }
    /// <summary>
    /// Подписывает HUD на изменение здоровья и уровня игрока
    /// </summary>
    private void SubscribeStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged += UpdateHp;
        playerStats.OnLevelChanged += UpdateLevel;
    }
    /// <summary>
    /// Отписывает HUD от всех игровых событий
    /// </summary>
    private void Unsubscribe()
    {
        UnsubscribeStats();
        UnsubscribeInventory();
    }
    /// <summary>
    /// Отписывает HUD от изменения здоровья и уровня игрока
    /// </summary>
    private void UnsubscribeStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= UpdateHp;
        playerStats.OnLevelChanged -= UpdateLevel;
    }
    /// <summary>
    /// Подписывает HUD на изменение экипировки игрока
    /// </summary>
    private void SubscribeInventory()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnEquipmentChanged += UpdateEquipmentSlots;
    }
    /// <summary>
    /// Отписывает HUD от изменения экипировки игрока
    /// </summary>
    private void UnsubscribeInventory()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnEquipmentChanged -= UpdateEquipmentSlots;
    }
    /// <summary>
    /// Обновляет сердечки здоровья игрока
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
    /// Обновляет значок уровня игрока
    /// </summary>
    private void UpdateLevel(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LVL {newLevel}";
    }
    /// <summary>
    /// Перерисовывает ячейки экипировки в HUD
    /// </summary>
    private void RefreshSlots()
    {
        UpdateEquipmentSlots(playerInventory != null ? playerInventory.GetEquippedItems() : null);
    }
    /// <summary>
    /// Раскладывает надетые предметы по ячейкам HUD
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
    /// Подписывает ячейки HUD на нажатие крестика снятия предмета
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
    /// Отписывает ячейки HUD от нажатия крестика снятия предмета
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
    /// Снимает предмет, крестик которого нажал игрок в HUD
    /// </summary>
    private void HandleInventorySlotRemoveClicked(ItemData item)
    {
        if (playerInventory != null)
            playerInventory.Unequip(item);
    }
}
