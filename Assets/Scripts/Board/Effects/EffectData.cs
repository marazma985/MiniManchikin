using System;
using UnityEngine;
/// <summary>
/// Описывает или применяет игровой эффект, связанный с EffectData
/// </summary>

[Serializable]
public sealed class EffectData
{
    [SerializeField] private EffectType effectType;
    [SerializeField] private int value;
    [SerializeField] private string contentId;
    [SerializeField] private Rarity rarityFilter;
    [SerializeField] private TileType targetTileType;
    [SerializeField] private bool useNearestMatchingTile;
    [SerializeField] private bool useRandomTarget;
    [SerializeField] private bool isInstant;
    [SerializeField] private bool restoreToFull;

    /// <summary>
    /// Создает пустой эффект для сериализации Unity и ручной настройки в инспекторе
    /// </summary>
    public EffectData()
    {
    }
    /// <summary>
    /// Создает экземпляр EffectData и заполняет его начальными данными
    /// </summary>
    public EffectData(EffectType effectType, int value)
    {
        this.effectType = effectType;
        this.value = value;
    }
    /// <summary>
    /// Создает экземпляр EffectData и заполняет его начальными данными
    /// </summary>
    public EffectData(EffectType effectType, int value, Rarity rarityFilter, bool useRandomTarget)
    {
        this.effectType = effectType;
        this.value = value;
        this.rarityFilter = rarityFilter;
        this.useRandomTarget = useRandomTarget;
    }
    /// <summary>
    /// Создает экземпляр EffectData и заполняет его начальными данными
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
    public TileType TargetTileType => targetTileType;
    public bool UseNearestMatchingTile => useNearestMatchingTile;
    public bool UseRandomTarget => useRandomTarget;
    public bool IsInstant => isInstant;
    public bool RestoreToFull => restoreToFull;
}
