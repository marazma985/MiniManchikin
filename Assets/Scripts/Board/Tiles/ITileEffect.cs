/// <summary>
/// Задает общий контракт itile effect, чтобы разные реализации можно было вызывать одинаково
/// </summary>
public interface ITileEffect
{
    void Resolve(BoardTile tile);
}
