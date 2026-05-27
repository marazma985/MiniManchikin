using System;
/// <summary>
/// Общее правило для клеток, эффект которых может завершиться не сразу из-за окна или награды
/// </summary>

public interface IDeferredTileEffect : ITileEffect
{
    void Resolve(BoardTile tile, Action onResolved);
}
