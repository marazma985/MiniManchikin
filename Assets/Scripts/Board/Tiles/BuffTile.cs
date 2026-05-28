using System;

public sealed class BuffTile : BoardTile
{
    public override bool MatchesTargetId(string targetTileId)
    {
        return TileTargetIds.Matches(targetTileId, TileTargetIds.Buff);
    }

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = TileEffectSelection.PickRandom(context?.BuffEvents);
        TileEffectSelection.ApplyOrComplete(context, effect, "Buff tile", onResolved);
    }
}
