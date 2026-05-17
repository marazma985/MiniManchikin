using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    private const int MaxEquippedItems = 3;

    [SerializeField] private List<ItemData> equippedItems = new List<ItemData>(MaxEquippedItems);
    [SerializeField] private ItemData testDagger;
    [SerializeField] private ItemData testHelmet;
    [SerializeField] private ItemData testNecklace;
    [SerializeField] private ItemData testWingedBoot;

    public event Action<IReadOnlyList<ItemData>> OnEquipmentChanged;

    public int MaxItems => MaxEquippedItems;
    public int EquippedCount => equippedItems.Count;

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

    public void ClearEquipment()
    {
        if (equippedItems.Count == 0)
            return;

        equippedItems.Clear();
        Debug.Log("Equipment cleared.");
        NotifyEquipmentChanged();
    }

    public bool HasFreeSlot()
    {
        return equippedItems.Count < MaxEquippedItems;
    }

    public IReadOnlyList<ItemData> GetEquippedItems()
    {
        return equippedItems;
    }

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

    [ContextMenu("Test Equip Dagger")]
    private void TestEquipDagger()
    {
        TryEquip(testDagger);
    }

    [ContextMenu("Test Equip Helmet")]
    private void TestEquipHelmet()
    {
        TryEquip(testHelmet);
    }

    [ContextMenu("Test Equip Necklace")]
    private void TestEquipNecklace()
    {
        TryEquip(testNecklace);
    }

    [ContextMenu("Test Equip Winged Boot")]
    private void TestEquipWingedBoot()
    {
        TryEquip(testWingedBoot);
    }

    [ContextMenu("Test Clear Equipment")]
    private void TestClearEquipment()
    {
        ClearEquipment();
    }

    private void OnEnable()
    {
        NotifyEquipmentChanged();
    }

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

    private void NotifyEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke(equippedItems);
    }
}
