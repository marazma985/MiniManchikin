using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// Управляет ходом игрока на поле: броском кубика, движением, разрешением клетки и продолжением сохраненного хода
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
    /// Пытается начать обычный ход игрока с броском кубика
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
    /// Пытается сдвинуть игрока на заранее известное число клеток без показа анимации броска
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
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetSystems(DiceSystem newDiceSystem, PlayerMover newPlayerMover, BoardManager newBoardManager, TileEffectSystem newTileEffectSystem)
    {
        diceSystem = newDiceSystem;
        playerMover = newPlayerMover;
        boardManager = newBoardManager;
        tileEffectSystem = newTileEffectSystem;
    }
    /// <summary>
    /// Собирает снимок текущего состояния, чтобы продолжение игры вернулось к тому же моменту
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
    /// Восстанавливает состояние из сохранения без повторной генерации случайных исходов
    /// </summary>
    public void RestoreFromSave(TurnSaveData saveData, bool hasActiveModal)
    {
        if (saveData == null)
        {
            SetState(TurnState.WaitingForRoll);
            return;
        }

        // Если ход не заблокирован модальным окном, продолжается именно сохраненное движение с сохраненной стартовой клетки
        if (saveData.hasPendingBoardMove && !hasActiveModal)
        {
            hasPendingBoardMove = true;
            pendingBoardMoveSteps = Mathf.Max(0, saveData.pendingBoardMoveSteps);
            pendingBoardMoveShowsDice = saveData.pendingBoardMoveShowsDice;
            pendingBoardMoveStartTileIndex = saveData.pendingBoardMoveStartTileIndex;
            boardManager?.SetCurrentIndex(pendingBoardMoveStartTileIndex);
            playerMover?.SnapToCurrentTile();
            // Фиксированное движение используется картами и не должно показывать анимацию броска кубика на поле
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
    /// Доигрывает сохраненное разрешение клетки после загрузки продолжения
    /// </summary>
    public void CompleteRestoredTileResolution()
    {
        EndTurn();
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода StartTurn
    /// </summary>
    private void StartTurn()
    {
        if (diceSystem == null || playerMover == null || boardManager == null || tileEffectSystem == null || playerMover.IsMoving)
        {
            RollRejected?.Invoke();
            return;
        }

        SetState(TurnState.RollingDice);

        // Результат кубика сохраняется до анимации, поэтому закрытие процесса не дает перекинуть движение по полю
        var steps = diceSystem.Roll();
        SetPendingMove(steps, true);
        DiceRolled?.Invoke(steps);

        StartCoroutine(PlayDiceAnimationThenMove(steps));
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода StartTurnWithSteps
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
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    private IEnumerator PlayDiceAnimationThenMove(int steps)
    {
        yield return DiceRollAnimationPlayer.PlayGlobalRoutine(steps);
        StartMovement(steps);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода StartMovement
    /// </summary>
    private void StartMovement(int steps)
    {
        SetState(TurnState.MovingPlayer);
        playerMover.MoveSteps(steps, OnPlayerMoveCompleted);
    }
    /// <summary>
    /// Продолжает движение по полю, если игра была закрыта после броска, но до завершения хода
    /// </summary>
    private IEnumerator ResumePendingMove()
    {
        if (pendingBoardMoveShowsDice)
            yield return DiceRollAnimationPlayer.PlayGlobalRoutine(pendingBoardMoveSteps);

        StartMovement(pendingBoardMoveSteps);
    }
    /// <summary>
    /// Реагирует на событие player move completed
    /// </summary>
    private void OnPlayerMoveCompleted()
    {
        ClearPendingMove();

        var currentTile = boardManager != null ? boardManager.CurrentTile : null;
        PlayerMoveCompleted?.Invoke(currentTile);

        // Эффекты клеток могут открывать окна, поэтому callback завершает ход только после настоящего разрешения эффекта
        SetState(TurnState.ResolvingTile);
        TileResolving?.Invoke(currentTile);

        tileEffectSystem.ResolveTile(currentTile, () =>
        {
            TileResolved?.Invoke(currentTile);
            EndTurn();
        });
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода EndTurn
    /// </summary>
    private void EndTurn()
    {
        ClearPendingMove();
        SetState(TurnState.TurnEnded);
        TurnEnded?.Invoke();
        SetState(TurnState.WaitingForRoll);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private void SetPendingMove(int steps, bool showsDice)
    {
        hasPendingBoardMove = true;
        pendingBoardMoveSteps = Mathf.Max(0, steps);
        pendingBoardMoveShowsDice = showsDice;
        pendingBoardMoveStartTileIndex = boardManager != null ? boardManager.CurrentIndex : 0;
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    private void ClearPendingMove()
    {
        hasPendingBoardMove = false;
        pendingBoardMoveSteps = 0;
        pendingBoardMoveShowsDice = false;
        pendingBoardMoveStartTileIndex = 0;
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private void SetState(TurnState newState)
    {
        if (state == newState)
            return;

        state = newState;
        StateChanged?.Invoke(state);
    }
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        diceSystem = FindAnyObjectByType<DiceSystem>();
        playerMover = FindAnyObjectByType<PlayerMover>();
        boardManager = FindAnyObjectByType<BoardManager>();
        tileEffectSystem = FindAnyObjectByType<TileEffectSystem>();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (state == TurnState.RollingDice || state == TurnState.MovingPlayer || state == TurnState.ResolvingTile || state == TurnState.TurnEnded)
            state = TurnState.WaitingForRoll;
    }
}
