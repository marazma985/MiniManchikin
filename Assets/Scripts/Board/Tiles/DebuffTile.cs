using System;

public sealed class DebuffTile : BoardTile
{
    public override bool MatchesTargetId(string targetTileId)
    {
        return TileTargetIds.Matches(targetTileId, TileTargetIds.Debuff);
    }

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = TileEffectSelection.PickRandom(context?.DebuffEvents);
        TileEffectSelection.ApplyOrComplete(context, effect, "Debuff tile", onResolved);
    }
}
