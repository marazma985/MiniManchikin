using System;

public sealed class DebuffTile : BoardTile
{
    public override TileType Type => TileType.Debuff;

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        var effect = TileEffectSelection.PickRandom(context?.DebuffEvents);
        TileEffectSelection.ApplyOrComplete(context, effect, "Debuff tile", onResolved);
    }
}
