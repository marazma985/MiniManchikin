using UnityEngine;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с HealTileEffect
/// </summary>

public sealed class HealTileEffect : ITileEffect
{
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Debug.Log("Heal tile resolved");
    }
}
