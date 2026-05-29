using System;
using UnityEngine;

public sealed class BattleTile : BoardTile
{
    public override TileType Type => TileType.Battle;

    public override void Resolve(TileResolutionContext context, Action onResolved)
    {
        if (context == null || context.BattleSystem == null)
        {
            Debug.LogWarning("Battle tile resolved, but BattleSystem is not assigned.");
            onResolved?.Invoke();
            return;
        }

        context.BattleSystem.StartBattle(onResolved);
    }
}
