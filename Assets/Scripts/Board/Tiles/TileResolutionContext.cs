using System.Collections.Generic;

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
