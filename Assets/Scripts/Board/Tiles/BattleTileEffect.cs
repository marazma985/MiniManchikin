using UnityEngine;
/// <summary>
/// Отвечает за клетки поля и их эффекты, связанные с BattleTileEffect
/// </summary>

public sealed class BattleTileEffect : IDeferredTileEffect
{
    private BattleSystem battleSystem;
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetBattleSystem(BattleSystem newBattleSystem)
    {
        battleSystem = newBattleSystem;
    }
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
    /// </summary>
    public void Resolve(BoardTile tile)
    {
        Resolve(tile, null);
    }
    /// <summary>
    /// Разрешает игровую ситуацию и переводит ее в следующее состояние
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
