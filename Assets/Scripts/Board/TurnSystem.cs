using System;
using UnityEngine;

public sealed class TurnSystem : MonoBehaviour
{
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private TileEffectSystem tileEffectSystem;
    [SerializeField] private TurnState state = TurnState.WaitingForRoll;

    public event Action<TurnState> StateChanged;
    public event Action<int> DiceRolled;
    public event Action<BoardTile> PlayerMoveCompleted;
    public event Action<BoardTile> TileResolving;
    public event Action<BoardTile> TileResolved;
    public event Action TurnEnded;
    public event Action RollRejected;

    public TurnState State => state;
    public bool CanRoll => state == TurnState.WaitingForRoll;

    public bool TryRollDice()
    {
        if (!CanRoll)
        {
            RollRejected?.Invoke();
            return false;
        }

        StartTurn();
        return true;
    }

    [ContextMenu("Test Move 3 Tiles")]
    public void TestMoveThreeTiles()
    {
        TryMoveFixedSteps(3);
    }

    [ContextMenu("Test Move 2 Tiles")]
    public void TestMoveTwoTiles()
    {
        TryMoveFixedSteps(2);
    }

    [ContextMenu("Test Move 1 Tiles")]
    public void TestMoveOneTiles()
    {
        TryMoveFixedSteps(1);
    }

    [ContextMenu("Test Move 6 Tiles")]
    public void TestMoveSixTiles()
    {
        TryMoveFixedSteps(6);
    }

    public bool TryMoveFixedSteps(int steps)
    {
        if (!CanRoll || steps <= 0)
        {
            RollRejected?.Invoke();
            return false;
        }

        StartTurnWithSteps(steps);
        return true;
    }

    public void SetSystems(DiceSystem newDiceSystem, PlayerMover newPlayerMover, BoardManager newBoardManager, TileEffectSystem newTileEffectSystem)
    {
        diceSystem = newDiceSystem;
        playerMover = newPlayerMover;
        boardManager = newBoardManager;
        tileEffectSystem = newTileEffectSystem;
    }

    private void StartTurn()
    {
        if (diceSystem == null || playerMover == null || boardManager == null || tileEffectSystem == null || playerMover.IsMoving)
        {
            RollRejected?.Invoke();
            return;
        }

        SetState(TurnState.RollingDice);

        var steps = diceSystem.Roll();
        DiceRolled?.Invoke(steps);

        StartMovement(steps);
    }

    private void StartTurnWithSteps(int steps)
    {
        if (playerMover == null || boardManager == null || tileEffectSystem == null || playerMover.IsMoving)
        {
            RollRejected?.Invoke();
            return;
        }

        SetState(TurnState.RollingDice);
        DiceRolled?.Invoke(steps);
        StartMovement(steps);
    }

    private void StartMovement(int steps)
    {
        SetState(TurnState.MovingPlayer);
        playerMover.MoveSteps(steps, OnPlayerMoveCompleted);
    }

    private void OnPlayerMoveCompleted()
    {
        var currentTile = boardManager != null ? boardManager.CurrentTile : null;
        PlayerMoveCompleted?.Invoke(currentTile);

        SetState(TurnState.ResolvingTile);
        TileResolving?.Invoke(currentTile);

        tileEffectSystem.ResolveTile(currentTile, () =>
        {
            TileResolved?.Invoke(currentTile);
            EndTurn();
        });
    }

    private void EndTurn()
    {
        SetState(TurnState.TurnEnded);
        TurnEnded?.Invoke();
        SetState(TurnState.WaitingForRoll);
    }

    private void SetState(TurnState newState)
    {
        if (state == newState)
            return;

        state = newState;
        StateChanged?.Invoke(state);
    }

    private void Reset()
    {
        diceSystem = FindAnyObjectByType<DiceSystem>();
        playerMover = FindAnyObjectByType<PlayerMover>();
        boardManager = FindAnyObjectByType<BoardManager>();
        tileEffectSystem = FindAnyObjectByType<TileEffectSystem>();
    }

    private void OnValidate()
    {
        if (state == TurnState.RollingDice || state == TurnState.MovingPlayer || state == TurnState.ResolvingTile || state == TurnState.TurnEnded)
            state = TurnState.WaitingForRoll;
    }
}
