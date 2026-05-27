using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// Управляет ходом на поле: броском кубика, движением фишки и запуском эффекта клетки
/// </summary>

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
    /// <summary>
    /// Пробует начать ход игрока обычным броском кубика
    /// </summary>
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
    /// <summary>
    /// Двигает игрока на заданное число клеток без анимации броска кубика
    /// </summary>
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
    /// <summary>
    /// Подключает систему хода к кубику, фишке, полю и эффектам клеток
    /// </summary>
    public void SetSystems(DiceSystem newDiceSystem, PlayerMover newPlayerMover, BoardManager newBoardManager, TileEffectSystem newTileEffectSystem)
    {
        diceSystem = newDiceSystem;
        playerMover = newPlayerMover;
        boardManager = newBoardManager;
        tileEffectSystem = newTileEffectSystem;
    }
    /// <summary>
    /// Собирает важные данные текущего состояния для файла сохранения
    /// </summary>
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
    /// <summary>
    /// Возвращает состояние игры из сохранения без новых случайных результатов
    /// </summary>
    public void RestoreFromSave(TurnSaveData saveData, bool hasActiveModal)
    {
        if (saveData == null)
        {
            SetState(TurnState.WaitingForRoll);
            return;
        }

        // Если игрок закрыл игру во время хода, движение продолжается с той же клетки
        if (saveData.hasPendingBoardMove && !hasActiveModal)
        {
            hasPendingBoardMove = true;
            pendingBoardMoveSteps = Mathf.Max(0, saveData.pendingBoardMoveSteps);
            pendingBoardMoveShowsDice = saveData.pendingBoardMoveShowsDice;
            pendingBoardMoveStartTileIndex = saveData.pendingBoardMoveStartTileIndex;
            boardManager?.SetCurrentIndex(pendingBoardMoveStartTileIndex);
            playerMover?.SnapToCurrentTile();
            // Карты могут двигать игрока на фиксированное число клеток без показа кубика
            SetState(TurnState.RollingDice);
            StartCoroutine(ResumePendingMove());
            return;
        }

        if (hasActiveModal)
            SetState((TurnState)Mathf.Clamp(saveData.state, (int)TurnState.WaitingForRoll, (int)TurnState.ResolvingTile));
        else
            SetState(TurnState.WaitingForRoll);
    }
    /// <summary>
    /// Завершает обработку клетки после восстановления сохранения
    /// </summary>
    public void CompleteRestoredTileResolution()
    {
        EndTurn();
    }
    /// <summary>
    /// Начинает обычный ход игрока на поле
    /// </summary>
    private void StartTurn()
    {
        if (diceSystem == null || playerMover == null || boardManager == null || tileEffectSystem == null || playerMover.IsMoving)
        {
            RollRejected?.Invoke();
            return;
        }

        SetState(TurnState.RollingDice);

        // Результат кубика запоминается до анимации, чтобы перезапуск игры не позволял перекинуть ход
        var steps = diceSystem.Roll();
        SetPendingMove(steps, true);
        DiceRolled?.Invoke(steps);

        StartCoroutine(PlayDiceAnimationThenMove(steps));
    }
    /// <summary>
    /// Начинает ход с заранее известным числом шагов, например после карты
    /// </summary>
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
    /// <summary>
    /// Проигрывает анимацию выпавшего кубика и затем начинает движение игрока
    /// </summary>
    private IEnumerator PlayDiceAnimationThenMove(int steps)
    {
        yield return DiceRollAnimationPlayer.PlayGlobalRoutine(steps);
        StartMovement(steps);
    }
    /// <summary>
    /// Запускает перемещение игрока на выпавшее количество клеток
    /// </summary>
    private void StartMovement(int steps)
    {
        SetState(TurnState.MovingPlayer);
        playerMover.MoveSteps(steps, OnPlayerMoveCompleted);
    }
    /// <summary>
    /// Продолжает движение после загрузки, если игра была закрыта посреди хода
    /// </summary>
    private IEnumerator ResumePendingMove()
    {
        if (pendingBoardMoveShowsDice)
            yield return DiceRollAnimationPlayer.PlayGlobalRoutine(pendingBoardMoveSteps);

        StartMovement(pendingBoardMoveSteps);
    }
    /// <summary>
    /// Продолжает ход после того, как фишка дошла до клетки
    /// </summary>
    private void OnPlayerMoveCompleted()
    {
        ClearPendingMove();

        var currentTile = boardManager != null ? boardManager.CurrentTile : null;
        PlayerMoveCompleted?.Invoke(currentTile);

        // Клетка может открыть окно, поэтому ход завершается только после окончания эффекта
        SetState(TurnState.ResolvingTile);
        TileResolving?.Invoke(currentTile);

        tileEffectSystem.ResolveTile(currentTile, () =>
        {
            TileResolved?.Invoke(currentTile);
            EndTurn();
        });
    }
    /// <summary>
    /// Завершает ход и снова разрешает бросать кубик
    /// </summary>
    private void EndTurn()
    {
        ClearPendingMove();
        SetState(TurnState.TurnEnded);
        TurnEnded?.Invoke();
        SetState(TurnState.WaitingForRoll);
    }
    /// <summary>
    /// Запоминает незавершенный ход, чтобы его можно было восстановить из сохранения
    /// </summary>
    private void SetPendingMove(int steps, bool showsDice)
    {
        hasPendingBoardMove = true;
        pendingBoardMoveSteps = Mathf.Max(0, steps);
        pendingBoardMoveShowsDice = showsDice;
        pendingBoardMoveStartTileIndex = boardManager != null ? boardManager.CurrentIndex : 0;
    }
    /// <summary>
    /// Сбрасывает сохраненный незавершенный ход после его завершения
    /// </summary>
    private void ClearPendingMove()
    {
        hasPendingBoardMove = false;
        pendingBoardMoveSteps = 0;
        pendingBoardMoveShowsDice = false;
        pendingBoardMoveStartTileIndex = 0;
    }
    /// <summary>
    /// Меняет текущее состояние хода и сообщает об этом кнопке кубика
    /// </summary>
    private void SetState(TurnState newState)
    {
        if (state == newState)
            return;

        state = newState;
        StateChanged?.Invoke(state);
    }
    /// <summary>
    /// Автоматически находит системы хода на сцене при добавлении компонента
    /// </summary>
    private void Reset()
    {
        diceSystem = FindAnyObjectByType<DiceSystem>();
        playerMover = FindAnyObjectByType<PlayerMover>();
        boardManager = FindAnyObjectByType<BoardManager>();
        tileEffectSystem = FindAnyObjectByType<TileEffectSystem>();
    }
    /// <summary>
    /// Ограничивает настройки движения допустимыми значениями в инспекторе
    /// </summary>
    private void OnValidate()
    {
        if (state == TurnState.RollingDice || state == TurnState.MovingPlayer || state == TurnState.ResolvingTile || state == TurnState.TurnEnded)
            state = TurnState.WaitingForRoll;
    }
}
