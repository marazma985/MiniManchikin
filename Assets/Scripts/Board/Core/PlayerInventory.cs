using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с PlayerInventory
/// </summary>

public sealed class PlayerInventory : MonoBehaviour
{
    private const int MaxEquippedItems = 3;

    [SerializeField] private List<ItemData> equippedItems = new List<ItemData>(MaxEquippedItems);

    public event Action<IReadOnlyList<ItemData>> OnEquipmentChanged;

    public int MaxItems => MaxEquippedItems;
    public int EquippedCount => equippedItems.Count;
    /// <summary>
    /// Пытается выполнить действие и возвращает, получилось ли это сделать
    /// </summary>
    public bool TryEquip(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot equip null item.");
            return false;
        }

        if (!HasFreeSlot())
        {
            Debug.LogWarning($"Cannot equip '{item.ItemName}'. Equipment limit reached ({MaxEquippedItems}).");
            return false;
        }

        equippedItems.Add(item);
        Debug.Log($"Equipped item: {item.ItemName}");
        NotifyEquipmentChanged();
        return true;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода Unequip
    /// </summary>
    public bool Unequip(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot unequip null item.");
            return false;
        }

        if (!equippedItems.Remove(item))
            return false;

        Debug.Log($"Unequipped item: {item.ItemName}");
        NotifyEquipmentChanged();
        return true;
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    public void ClearEquipment()
    {
        if (equippedItems.Count == 0)
            return;

        equippedItems.Clear();
        Debug.Log("Equipment cleared.");
        NotifyEquipmentChanged();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetEquipment(IReadOnlyList<ItemData> items)
    {
        equippedItems.Clear();

        if (items != null)
        {
            for (var i = 0; i < items.Count && equippedItems.Count < MaxEquippedItems; i++)
            {
                if (items[i] != null)
                    equippedItems.Add(items[i]);
            }
        }

        NotifyEquipmentChanged();
    }
    /// <summary>
    /// Проверяет, есть ли нужное состояние или данные
    /// </summary>
    public bool HasFreeSlot()
    {
        return equippedItems.Count < MaxEquippedItems;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    public IReadOnlyList<ItemData> GetEquippedItems()
    {
        return equippedItems;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    public int GetTotalEffectValue(EffectType effectType)
    {
        var total = 0;

        for (var i = 0; i < equippedItems.Count; i++)
        {
            var item = equippedItems[i];
            if (item == null || item.Effects == null)
                continue;

            var effects = item.Effects;
            for (var effectIndex = 0; effectIndex < effects.Count; effectIndex++)
            {
                var effect = effects[effectIndex];
                if (effect != null && effect.EffectType == effectType)
                    total += effect.Value;
            }
        }

        return total;
    }
    /// <summary>
    /// Пытается выполнить действие и возвращает, получилось ли это сделать
    /// </summary>
    public bool TryBreakArmorForHpLoss()
    {
        for (var i = 0; i < equippedItems.Count; i++)
        {
            var item = equippedItems[i];
            if (item == null || item.ItemType != ItemType.Armor)
                continue;

            equippedItems.RemoveAt(i);
            Debug.Log($"Armor broke and prevented HP loss: {item.ItemName}");
            NotifyEquipmentChanged();
            return true;
        }

        return false;
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        NotifyEquipmentChanged();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (equippedItems == null)
        {
            equippedItems = new List<ItemData>(MaxEquippedItems);
            return;
        }

        equippedItems.RemoveAll(item => item == null);
        if (equippedItems.Count > MaxEquippedItems)
            equippedItems.RemoveRange(MaxEquippedItems, equippedItems.Count - MaxEquippedItems);
    }
    /// <summary>
    /// Сообщает подписчикам, что состояние изменилось
    /// </summary>
    private void NotifyEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke(equippedItems);
    }
}
