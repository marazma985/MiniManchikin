using System;

public interface IBoardTile
{
    int Index { get; }
    TileType Type { get; }
    void Enter();
    void Resolve(TileResolutionContext context, Action onResolved);
    bool Matches(TileType targetTileType);
}
