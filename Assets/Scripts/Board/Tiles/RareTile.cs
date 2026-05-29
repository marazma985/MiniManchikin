using System;

public sealed class RareTile : BoardTile
{
    public override TileType Type => TileType.RareEvent;

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = TileEffectSelection.PickRandom(context?.RareEvents);
        TileEffectSelection.ApplyOrComplete(context, effect, "Rare event tile", onResolved);
    }
}
