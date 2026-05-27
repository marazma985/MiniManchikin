/// <summary>
/// Состояния хода игрока: ожидание броска, движение, обработка клетки и завершение хода
/// </summary>
public enum TurnState
{
    WaitingForRoll,
    RollingDice,
    MovingPlayer,
    ResolvingTile,
    TurnEnded
}
