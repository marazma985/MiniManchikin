using System;
using System.Collections.Generic;

public sealed class RandomEventTile : BoardTile
{
    public override bool MatchesTargetId(string targetTileId)
    {
        return TileTargetIds.Matches(targetTileId, TileTargetIds.RandomEvent);
    }

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = PickRandomEvent(context);
        TileEffectSelection.ApplyOrComplete(context, effect, "Random event tile", onResolved);
    }

    private static EffectData PickRandomEvent(TileResolutionContext context)
    {
        if (context == null)
            return null;

        var hasBuffEvents = HasEvents(context.BuffEvents);
        var hasDebuffEvents = HasEvents(context.DebuffEvents);

        if (!hasBuffEvents && !hasDebuffEvents)
            return null;

        if (hasBuffEvents && hasDebuffEvents)
            return UnityEngine.Random.Range(0, 2) == 0
                ? TileEffectSelection.PickRandom(context.BuffEvents)
                : TileEffectSelection.PickRandom(context.DebuffEvents);

        return hasBuffEvents
            ? TileEffectSelection.PickRandom(context.BuffEvents)
            : TileEffectSelection.PickRandom(context.DebuffEvents);
    }

    private static bool HasEvents(IReadOnlyList<EffectData> effects)
    {
        return effects != null && effects.Count > 0;
    }
}
