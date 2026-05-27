/// <summary>
/// Общее правило для всех эффектов клеток поля
/// </summary>
public interface ITileEffect
{
    void Resolve(BoardTile tile);
}
