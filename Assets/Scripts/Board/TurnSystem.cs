using System;
using System.Collections;
using UnityEngine;

public sealed class TurnSystem : MonoBehaviour
{
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private TileEffectSystem tileEffectSystem;
    [SerializeField] private TurnState state = TurnState.WaitingForRoll;

    private bool hasPendingBoardMove;
    private int pendingBoardMoveSteps;
    private bool pendingBoardMoveShowsDice;
    private int pendingBoardMoveStartTileIndex;

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

    public TurnSaveData CaptureSaveData()
    {
        return new TurnSaveData
        {
            state = (int)state,
            hasPendingBoardMove = hasPendingBoardMove,
            pendingBoardMoveSteps = pendingBoardMoveSteps,
            pendingBoardMoveShowsDice = pendingBoardMoveShowsDice,
            pendingBoardMoveStartTileIndex = pendingBoardMoveStartTileIndex
        };
    }

    public void RestoreFromSave(TurnSaveData saveData, bool hasActiveModal)
    {
        if (saveData == null)
        {
            SetState(TurnState.WaitingForRoll);
            return;
        }

        if (saveData.hasPendingBoardMove && !hasActiveModal)
        {
            hasPendingBoardMove = true;
            pendingBoardMoveSteps = Mathf.Max(0, saveData.pendingBoardMoveSteps);
            pendingBoardMoveShowsDice = saveData.pendingBoardMoveShowsDice;
            pendingBoardMoveStartTileIndex = saveData.pendingBoardMoveStartTileIndex;
            boardManager?.SetCurrentIndex(pendingBoardMoveStartTileIndex);
            playerMover?.SnapToCurrentTile();
            SetState(TurnState.RollingDice);
            StartCoroutine(ResumePendingMove());
            return;
        }

        if (hasActiveModal)
            SetState((TurnState)Mathf.Clamp(saveData.state, (int)TurnState.WaitingForRoll, (int)TurnState.ResolvingTile));
        else
            SetState(TurnState.WaitingForRoll);
    }

    public void CompleteRestoredTileResolution()
    {
        EndTurn();
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
        SetPendingMove(steps, true);
        DiceRolled?.Invoke(steps);

        StartCoroutine(PlayDiceAnimationThenMove(steps));
    }

    private void StartTurnWithSteps(int steps)
    {
        if (playerMover == null || boardManager == null || tileEffectSystem == null || playerMover.IsMoving)
        {
            RollRejected?.Invoke();
            return;
        }

        SetState(TurnState.RollingDice);
        SetPendingMove(steps, false);
        DiceRolled?.Invoke(steps);
        StartMovement(steps);
    }

    private IEnumerator PlayDiceAnimationThenMove(int steps)
    {
        yield return DiceRollAnimationPlayer.PlayGlobalRoutine(steps);
        StartMovement(steps);
    }

    private void StartMovement(int steps)
    {
        SetState(TurnState.MovingPlayer);
        playerMover.MoveSteps(steps, OnPlayerMoveCompleted);
    }

    private IEnumerator ResumePendingMove()
    {
        if (pendingBoardMoveShowsDice)
            yield return DiceRollAnimationPlayer.PlayGlobalRoutine(pendingBoardMoveSteps);

        StartMovement(pendingBoardMoveSteps);
    }

    private void OnPlayerMoveCompleted()
    {
        ClearPendingMove();

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
        ClearPendingMove();
        SetState(TurnState.TurnEnded);
        TurnEnded?.Invoke();
        SetState(TurnState.WaitingForRoll);
    }

    private void SetPendingMove(int steps, bool showsDice)
    {
        hasPendingBoardMove = true;
        pendingBoardMoveSteps = Mathf.Max(0, steps);
        pendingBoardMoveShowsDice = showsDice;
        pendingBoardMoveStartTileIndex = boardManager != null ? boardManager.CurrentIndex : 0;
    }

    private void ClearPendingMove()
    {
        hasPendingBoardMove = false;
        pendingBoardMoveSteps = 0;
        pendingBoardMoveShowsDice = false;
        pendingBoardMoveStartTileIndex = 0;
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
