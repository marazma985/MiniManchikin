using System;

public sealed class RareTile : BoardTile
{
    public override bool MatchesTargetId(string targetTileId)
    {
        return TileTargetIds.Matches(targetTileId, TileTargetIds.RareEvent);
    }

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = TileEffectSelection.PickRandom(context?.RareEvents);
        TileEffectSelection.ApplyOrComplete(context, effect, "Rare event tile", onResolved);
    }
}
