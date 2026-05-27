using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Определяет, что должно произойти после остановки игрока на конкретном типе клетки
/// </summary>

[RequireComponent(typeof(SingleRewardSystem))]
[RequireComponent(typeof(EventNotificationSystem))]
public sealed class TileEffectSystem : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private SingleRewardSystem singleRewardSystem;
    [SerializeField] private EventNotificationSystem eventNotificationSystem;
    [SerializeField] private List<CardData> possibleCommonCards = new List<CardData>();
    [SerializeField] private List<CardData> possibleRareCards = new List<CardData>();
    [SerializeField] private List<ItemData> possibleRareItems = new List<ItemData>();
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
    [SerializeField] private List<EffectData> rareEvents = new List<EffectData>
    {
        new EffectData(EffectType.GiveItem, 1, Rarity.Rare, true),
        new EffectData(EffectType.GiveCard, 1, Rarity.Rare, true),
        new EffectData(EffectType.HpRestore, 0, true),
        new EffectData(EffectType.Level, 2)
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
    /// <summary>
    /// Передает данные другой системе, чтобы она могла ими пользоваться
    /// </summary>
    public void RegisterEffect(TileType tileType, ITileEffect effect)
    {
        if (effect == null)
            return;

        effectsByTileType[tileType] = effect;
    }
    /// <summary>
    /// Передает системе сохранений данные, которые можно будет найти по id
    /// </summary>
    public void RegisterSaveContent(GameSaveContentResolver resolver)
    {
        if (resolver == null)
            return;

        RegisterCards(resolver, possibleCommonCards);
        RegisterCards(resolver, possibleRareCards);
        RegisterItems(resolver, possibleRareItems);
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    public void ResolveTile(BoardTile tile)
    {
        ResolveTile(tile, null);
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
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
    /// <summary>
    /// Завершает обработку клетки и передает ход дальше
    /// </summary>
    private void CompleteTileResolution(BoardTile tile, Action onResolved)
    {
        TileResolved?.Invoke(tile);
        onResolved?.Invoke();
    }
    /// <summary>
    /// Создает набор обработчиков для всех типов клеток
    /// </summary>
    private void Awake()
    {
        EnsureDefaultEvents();
        InitializeEffects();
    }
    /// <summary>
    /// Пересобирает обработчики клеток после правок в инспекторе
    /// </summary>
    private void OnValidate()
    {
        EnsureDefaultEvents();
        InitializeEffects();
    }
    /// <summary>
    /// Возвращает обработчик эффекта для типа клетки
    /// </summary>
    private ITileEffect GetEffect(TileType tileType)
    {
        if (effectsByTileType.Count == 0)
            InitializeEffects();

        return effectsByTileType.TryGetValue(tileType, out var effect) ? effect : eventTileEffect;
    }
    /// <summary>
    /// Подготавливает объект к работе
    /// </summary>
    private void InitializeEffects()
    {
        var resolvedSingleRewardSystem = ResolveSingleRewardSystem();
        var resolvedEventNotificationSystem = ResolveEventNotificationSystem();

        effectsByTileType.Clear();
        effectResolver.Configure(playerStats, playerInventory, cardSystem, resolvedSingleRewardSystem, resolvedEventNotificationSystem, possibleCommonCards, possibleRareCards, possibleRareItems);
        eventTileEffect.Configure(effectResolver, buffEvents, debuffEvents);
        buffTileEffect.Configure(effectResolver, buffEvents);
        debuffTileEffect.Configure(effectResolver, debuffEvents);
        rareTileEffect.Configure(effectResolver, rareEvents);
        battleTileEffect.SetBattleSystem(battleSystem);
        RegisterEffect(TileType.RandomEvent, eventTileEffect);
        RegisterEffect(TileType.RareEvent, rareTileEffect);
        RegisterEffect(TileType.Battle, battleTileEffect);
        RegisterEffect(TileType.Buff, buffTileEffect);
        RegisterEffect(TileType.Debuff, debuffTileEffect);

        _ = healTileEffect;
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    private SingleRewardSystem ResolveSingleRewardSystem()
    {
        if (singleRewardSystem != null)
            return singleRewardSystem;

        if (TryGetComponent(out SingleRewardSystem localSingleRewardSystem))
        {
            singleRewardSystem = localSingleRewardSystem;
            return singleRewardSystem;
        }

        Debug.LogWarning("TileEffectSystem requires SingleRewardSystem for card and item tile rewards.");
        return null;
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    private EventNotificationSystem ResolveEventNotificationSystem()
    {
        if (eventNotificationSystem != null)
            return eventNotificationSystem;

        if (TryGetComponent(out EventNotificationSystem localEventNotificationSystem))
        {
            eventNotificationSystem = localEventNotificationSystem;
            return eventNotificationSystem;
        }

        Debug.LogWarning("TileEffectSystem requires EventNotificationSystem for tile event feedback.");
        return null;
    }
    /// <summary>
    /// Создает или находит то, без чего объект не сможет работать
    /// </summary>
    private void EnsureDefaultEvents()
    {
        if (possibleCommonCards == null)
            possibleCommonCards = new List<CardData>();

        if (possibleRareCards == null)
            possibleRareCards = new List<CardData>();

        if (possibleRareItems == null)
            possibleRareItems = new List<ItemData>();

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

        if (rareEvents == null)
            rareEvents = new List<EffectData>();

        if (rareEvents.Count == 0)
        {
            rareEvents.Add(new EffectData(EffectType.GiveItem, 1, Rarity.Rare, true));
            rareEvents.Add(new EffectData(EffectType.GiveCard, 1, Rarity.Rare, true));
            rareEvents.Add(new EffectData(EffectType.HpRestore, 0, true));
            rareEvents.Add(new EffectData(EffectType.Level, 2));
        }
    }
    /// <summary>
    /// Передает данные другой системе, чтобы она могла ими пользоваться
    /// </summary>
    private static void RegisterCards(GameSaveContentResolver resolver, IReadOnlyList<CardData> cards)
    {
        if (cards == null)
            return;

        for (var i = 0; i < cards.Count; i++)
            resolver.AddCard(cards[i]);
    }
    /// <summary>
    /// Передает данные другой системе, чтобы она могла ими пользоваться
    /// </summary>
    private static void RegisterItems(GameSaveContentResolver resolver, IReadOnlyList<ItemData> items)
    {
        if (items == null)
            return;

        for (var i = 0; i < items.Count; i++)
            resolver.AddItem(items[i]);
    }
}
