using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Хранит надетые предметы игрока и считает бонусы, которые они дают
/// </summary>

public sealed class PlayerInventory : MonoBehaviour
{
    private const int MaxEquippedItems = 3;

    [SerializeField] private List<ItemData> equippedItems = new List<ItemData>(MaxEquippedItems);

    public event Action<IReadOnlyList<ItemData>> OnEquipmentChanged;

    public int MaxItems => MaxEquippedItems;
    public int EquippedCount => equippedItems.Count;
    /// <summary>
    /// Пытается надеть предмет, если в экипировке есть свободное место
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
    /// Снимает предмет с игрока и обновляет инвентарь
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
    /// Заменяет экипировку игрока списком предметов из сохранения
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
    /// Проверяет, осталось ли место для нового предмета экипировки
    /// </summary>
    public bool HasFreeSlot()
    {
        return equippedItems.Count < MaxEquippedItems;
    }
    /// <summary>
    /// Возвращает список предметов, которые сейчас надеты на игрока
    /// </summary>
    public IReadOnlyList<ItemData> GetEquippedItems()
    {
        return equippedItems;
    }
    /// <summary>
    /// Считает общий бонус экипировки для указанного типа эффекта
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
    /// Пытается сломать броню вместо потери здоровья
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
    /// Сообщает HUD о текущей экипировке при включении инвентаря
    /// </summary>
    private void OnEnable()
    {
        NotifyEquipmentChanged();
    }
    /// <summary>
    /// Убирает лишние предметы, если в инспекторе экипировка превысила лимит
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
    /// Сообщает HUD и другим системам, что экипировка игрока изменилась
    /// </summary>
    private void NotifyEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke(equippedItems);
    }
}
