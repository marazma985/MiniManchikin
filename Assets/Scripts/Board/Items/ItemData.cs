using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
/// <summary>
/// Настройки предмета: название, картинка, тип, редкость и бонусы
/// </summary>

[CreateAssetMenu(fileName = "ItemData", menuName = "Board Game/Item Data")]
public sealed class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Rarity rarity;
    [SerializeField, FormerlySerializedAs("equipmentSlot")] private ItemType itemType;
    [SerializeField] private List<EffectData> effects = new List<EffectData>();

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite ItemSprite => itemSprite;
    public Rarity Rarity => rarity;
    public ItemType ItemType => itemType;
    public IReadOnlyList<EffectData> Effects => effects;
    /// <summary>
    /// Автоматически подставляет id предмета по имени ассета, если id пустой
    /// </summary>
    private void OnValidate()
    {
        if (effects == null)
            effects = new List<EffectData>();
    }
}
