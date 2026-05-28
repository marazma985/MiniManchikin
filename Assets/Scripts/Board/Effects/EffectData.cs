using System;
using UnityEngine;
/// <summary>
/// Описание одного игрового эффекта: лечение, урон, уровень, награда, перемещение или бонус
/// </summary>

[Serializable]
public sealed class EffectData
{
    [SerializeField] private EffectType effectType;
    [SerializeField] private int value;
    [SerializeField] private string contentId;
    [SerializeField] private Rarity rarityFilter;
    [SerializeField] private string targetTileId;
    [SerializeField] private bool useNearestMatchingTile;
    [SerializeField] private bool useRandomTarget;
    [SerializeField] private bool isInstant;
    [SerializeField] private bool restoreToFull;

    /// <summary>
    /// Создает пустой эффект для настройки через инспектор Unity
    /// </summary>
    public EffectData()
    {
    }
    /// <summary>
    /// Создает обычный эффект с типом и числовым значением
    /// </summary>
    public EffectData(EffectType effectType, int value)
    {
        this.effectType = effectType;
        this.value = value;
    }
    /// <summary>
    /// Создает эффект, который может выбирать случайную цель нужной редкости
    /// </summary>
    public EffectData(EffectType effectType, int value, Rarity rarityFilter, bool useRandomTarget)
    {
        this.effectType = effectType;
        this.value = value;
        this.rarityFilter = rarityFilter;
        this.useRandomTarget = useRandomTarget;
    }
    /// <summary>
    /// Создает эффект лечения с возможностью восстановить здоровье полностью
    /// </summary>
    public EffectData(EffectType effectType, int value, bool restoreToFull)
    {
        this.effectType = effectType;
        this.value = value;
        this.restoreToFull = restoreToFull;
    }

    public EffectType EffectType => effectType;
    public int Value => value;
    public string ContentId => contentId;
    public Rarity RarityFilter => rarityFilter;
    public string TargetTileId => targetTileId;
    public TileTargetQuery TargetTileQuery => TileTargetQuery.ForId(targetTileId);
    public bool UseNearestMatchingTile => useNearestMatchingTile;
    public bool UseRandomTarget => useRandomTarget;
    public bool IsInstant => isInstant;
    public bool RestoreToFull => restoreToFull;
}
