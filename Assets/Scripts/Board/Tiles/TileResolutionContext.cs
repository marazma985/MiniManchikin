using System.Collections.Generic;

/// <summary>
/// класс для предоставления данных и систем для выполнения действия (объект-контейнер с зависимостями)
/// </summary>
public sealed class TileResolutionContext
{
    public TileResolutionContext(
        BattleSystem battleSystem,
        EffectResolver effectResolver,
        IReadOnlyList<EffectData> buffEvents,
        IReadOnlyList<EffectData> debuffEvents,
        IReadOnlyList<EffectData> rareEvents)
    {
        BattleSystem = battleSystem;
        EffectResolver = effectResolver;
        BuffEvents = buffEvents;
        DebuffEvents = debuffEvents;
        RareEvents = rareEvents;
    }

    public BattleSystem BattleSystem { get; }
    public EffectResolver EffectResolver { get; }
    public IReadOnlyList<EffectData> BuffEvents { get; }
    public IReadOnlyList<EffectData> DebuffEvents { get; }
    public IReadOnlyList<EffectData> RareEvents { get; }
}
