using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Двигает фишку игрока по клеткам поля после броска кубика или действия карты
/// </summary>

public sealed class PlayerMover : MonoBehaviour
{
    /// <summary>
    /// Событие, которое сообщает игре, на какой клетке закончилась остановка фишки
    /// </summary>
    [Serializable]
    public sealed class MoveCompletedEvent : UnityEvent<BoardTile>
    {
    }

    [SerializeField] private BoardManager boardManager;
    [SerializeField, HideInInspector] private float movementSpeed = 4f;
    [SerializeField, Min(0.01f)] private float stepDuration = 0.3f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float stopDuration = 0.08f;
    [SerializeField] private Vector3 tileLocalOffset;
    [SerializeField] private MoveCompletedEvent onMoveCompleted = new MoveCompletedEvent();

    private Coroutine moveCoroutine;
    private Action moveCompletedCallback;

    public bool IsMoving => moveCoroutine != null;
    public BoardManager BoardManager => boardManager;
    public float MovementSpeed => movementSpeed;
    public float StepDuration => stepDuration;
    public AnimationCurve MovementCurve => movementCurve;
    public MoveCompletedEvent OnMoveCompleted => onMoveCompleted;
    /// <summary>
    /// Перемещает объект или игровое состояние
    /// </summary>
    public void MoveSteps(int steps)
    {
        MoveSteps(steps, null);
    }
    /// <summary>
    /// Перемещает объект или игровое состояние
    /// </summary>
    public void MoveSteps(int steps, Action onCompleted)
    {
        if (IsMoving)
            return;

        moveCompletedCallback = onCompleted;
        moveCoroutine = StartCoroutine(MoveStepsRoutine(Mathf.Max(0, steps)));
    }
    /// <summary>
    /// Подключает движение фишки к указанному менеджеру поля
    /// </summary>
    public void SetBoardManager(BoardManager newBoardManager)
    {
        boardManager = newBoardManager;
    }
    /// <summary>
    /// Мгновенно ставит фигурку игрока на текущую клетку поля
    /// </summary>
    public void SnapToCurrentTile()
    {
        if (boardManager == null || boardManager.CurrentTile == null)
            return;

        var tile = boardManager.CurrentTile;
        transform.SetParent(tile.transform, true);
        transform.localPosition = tileLocalOffset;
    }
    /// <summary>
    /// Перемещает объект или игровое состояние
    /// </summary>
    private IEnumerator MoveStepsRoutine(int steps)
    {
        if (boardManager == null)
        {
            CompleteMove(null);
            yield break;
        }

        BoardTile currentTile = null;

        for (var i = 0; i < steps; i++)
        {
            currentTile = boardManager.AdvanceToNextTile();
            if (currentTile == null)
                break;

            yield return MoveToTile(currentTile);

            if (stopDuration > 0f && i < steps - 1)
                yield return new WaitForSeconds(stopDuration);
        }

        CompleteMove(currentTile ?? boardManager.CurrentTile);
    }
    /// <summary>
    /// Перемещает объект или игровое состояние
    /// </summary>
    private IEnumerator MoveToTile(BoardTile tile)
    {
        var startPosition = transform.position;
        var targetPosition = tile.transform.TransformPoint(tileLocalOffset);
        var elapsed = 0f;

        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / stepDuration);
            var curveTime = movementCurve != null ? movementCurve.Evaluate(normalizedTime) : normalizedTime;
            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, curveTime);
            yield return null;
        }

        transform.SetParent(tile.transform, true);
        transform.localPosition = tileLocalOffset;
    }
    /// <summary>
    /// Завершает перемещение фигурки и сообщает о достигнутой клетке
    /// </summary>
    private void CompleteMove(BoardTile finalTile)
    {
        moveCoroutine = null;
        onMoveCompleted.Invoke(finalTile);
        moveCompletedCallback?.Invoke();
        moveCompletedCallback = null;
    }
    /// <summary>
    /// Запоминает стартовое смещение фишки относительно клетки
    /// </summary>
    private void Reset()
    {
        boardManager = FindAnyObjectByType<BoardManager>();
        CacheTileLocalOffset();
    }
    /// <summary>
    /// Ограничивает длительность шага и обновляет смещение фишки в инспекторе
    /// </summary>
    private void OnValidate()
    {
        if (stepDuration <= 0f && movementSpeed > 0f)
            stepDuration = 1f / movementSpeed;

        stepDuration = Mathf.Max(0.01f, stepDuration);

        if (movementCurve == null || movementCurve.length == 0)
            movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();

        if (tileLocalOffset == Vector3.zero)
            CacheTileLocalOffset();
    }
    /// <summary>
    /// Сохраняет стартовый масштаб фигурки игрока для анимации движения
    /// </summary>
    private void Awake()
    {
        if (tileLocalOffset == Vector3.zero)
            CacheTileLocalOffset();
    }
    /// <summary>
    /// Запоминает смещение фишки относительно клетки поля
    /// </summary>
    private void CacheTileLocalOffset()
    {
        if (transform.parent == null)
            return;

        if (transform.parent.GetComponent<BoardTile>() == null)
            return;

        tileLocalOffset = transform.localPosition;
    }
}
