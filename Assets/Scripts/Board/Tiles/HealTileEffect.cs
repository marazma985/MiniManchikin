using UnityEngine;
/// <summary>
/// Эффект лечебной клетки, который сообщает о попадании на клетку лечения
/// </summary>

public sealed class HealTileEffect : ITileEffect
{
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Debug.Log("Heal tile resolved");
    }
}
