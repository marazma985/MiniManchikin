using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TileEffectSystem : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private List<CardData> possibleCommonCards = new List<CardData>();
    [SerializeField] private List<EffectData> buffEvents = new List<EffectData>
    {
        new EffectData(EffectType.HpRestore, 1),
        new EffectData(EffectType.HpRestore, 2),
        new EffectData(EffectType.GiveCard, 1, Rarity.Common, true),
        new EffectData(EffectType.Level, 1)
    };
    [SerializeField] private List<EffectData> debuffEvents = new List<EffectData>
    {
        new EffectData(EffectType.HpRestore, -1),
        new EffectData(EffectType.HpRestore, -2),
        new EffectData(EffectType.Level, -1),
        new EffectData(EffectType.RemoveCard, 1, Rarity.Common, true)
    };

    private readonly EffectResolver effectResolver = new EffectResolver();
    private readonly HealTileEffect healTileEffect = new HealTileEffect();
    private readonly DebuffTileEffect debuffTileEffect = new DebuffTileEffect();
    private readonly BattleTileEffect battleTileEffect = new BattleTileEffect();
    private readonly EventTileEffect eventTileEffect = new EventTileEffect();
    private readonly RareTileEffect rareTileEffect = new RareTileEffect();
    private readonly BuffTileEffect buffTileEffect = new BuffTileEffect();
    private readonly Dictionary<TileType, ITileEffect> effectsByTileType = new Dictionary<TileType, ITileEffect>();

    public event Action<BoardTile> TileResolving;
    public event Action<BoardTile> TileResolved;

    public void RegisterEffect(TileType tileType, ITileEffect effect)
    {
        if (effect == null)
            return;

        effectsByTileType[tileType] = effect;
    }

    public void ResolveTile(BoardTile tile)
    {
        ResolveTile(tile, null);
    }

    public void ResolveTile(BoardTile tile, Action onResolved)
    {
        TileResolving?.Invoke(tile);

        if (tile != null)
        {
            tile.Enter();
            var effect = GetEffect(tile.TileType);
            if (effect is IDeferredTileEffect deferredTileEffect)
            {
                deferredTileEffect.Resolve(tile, () => CompleteTileResolution(tile, onResolved));
                return;
            }

            effect.Resolve(tile);
        }
        else
        {
            Debug.LogWarning("TileEffectSystem received null tile.");
        }

        CompleteTileResolution(tile, onResolved);
    }

    private void CompleteTileResolution(BoardTile tile, Action onResolved)
    {
        TileResolved?.Invoke(tile);
        onResolved?.Invoke();
    }

    private void Awake()
    {
        EnsureDefaultEvents();
        InitializeEffects();
    }

    private void OnValidate()
    {
        EnsureDefaultEvents();
        InitializeEffects();
    }

    private ITileEffect GetEffect(TileType tileType)
    {
        if (effectsByTileType.Count == 0)
            InitializeEffects();

        return effectsByTileType.TryGetValue(tileType, out var effect) ? effect : eventTileEffect;
    }

    private void InitializeEffects()
    {
        effectsByTileType.Clear();
        effectResolver.Configure(playerStats, playerInventory, cardSystem, possibleCommonCards);
        eventTileEffect.Configure(effectResolver, buffEvents, debuffEvents);
        buffTileEffect.Configure(effectResolver, buffEvents);
        debuffTileEffect.Configure(effectResolver, debuffEvents);
        battleTileEffect.SetBattleSystem(battleSystem);
        RegisterEffect(TileType.RandomEvent, eventTileEffect);
        RegisterEffect(TileType.RareEvent, rareTileEffect);
        RegisterEffect(TileType.Battle, battleTileEffect);
        RegisterEffect(TileType.Buff, buffTileEffect);
        RegisterEffect(TileType.Debuff, debuffTileEffect);

        _ = healTileEffect;
    }

    private void EnsureDefaultEvents()
    {
        if (possibleCommonCards == null)
            possibleCommonCards = new List<CardData>();

        if (buffEvents == null)
            buffEvents = new List<EffectData>();

        if (buffEvents.Count == 0)
        {
            buffEvents.Add(new EffectData(EffectType.HpRestore, 1));
            buffEvents.Add(new EffectData(EffectType.HpRestore, 2));
            buffEvents.Add(new EffectData(EffectType.GiveCard, 1, Rarity.Common, true));
            buffEvents.Add(new EffectData(EffectType.Level, 1));
        }

        if (debuffEvents == null)
            debuffEvents = new List<EffectData>();

        if (debuffEvents.Count == 0)
        {
            debuffEvents.Add(new EffectData(EffectType.HpRestore, -1));
            debuffEvents.Add(new EffectData(EffectType.HpRestore, -2));
            debuffEvents.Add(new EffectData(EffectType.Level, -1));
            debuffEvents.Add(new EffectData(EffectType.RemoveCard, 1, Rarity.Common, true));
        }
    }
}
