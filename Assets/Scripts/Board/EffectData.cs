using System;
using UnityEngine;

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

    public EffectType EffectType => effectType;
    public int Value => value;
    public string ContentId => contentId;
    public Rarity RarityFilter => rarityFilter;
    public TileType TargetTileType => targetTileType;
    public bool UseNearestMatchingTile => useNearestMatchingTile;
    public bool UseRandomTarget => useRandomTarget;
    public bool IsInstant => isInstant;
}
