using UnityEngine;
/// <summary>
/// Эффект боевой клетки, который открывает бой с монстром
/// </summary>

public sealed class BattleTileEffect : IDeferredTileEffect
{
    private BattleSystem battleSystem;
    /// <summary>
    /// Подключает battle-клетку к системе боя
    /// </summary>
    public void SetBattleSystem(BattleSystem newBattleSystem)
    {
        battleSystem = newBattleSystem;
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Resolve(tile, null);
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    public void Resolve(BoardTile tile, System.Action onResolved)
    {
        if (battleSystem == null)
        {
            Debug.LogWarning("Battle tile resolved, but BattleSystem is not assigned.");
            onResolved?.Invoke();
            return;
        }

        battleSystem.StartBattle(onResolved);
    }
}
