using System;

public interface IBoardTile
{
    int Index { get; }
    void Enter();
    void Resolve(TileResolutionContext context, Action onResolved);
    bool Matches(TileTargetQuery query);
}
