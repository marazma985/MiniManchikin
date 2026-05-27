using System;
/// <summary>
/// Задает общий контракт ideferred tile effect, чтобы разные реализации можно было вызывать одинаково
/// </summary>

public interface IDeferredTileEffect : ITileEffect
{
    void Resolve(BoardTile tile, Action onResolved);
}
