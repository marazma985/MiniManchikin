/// <summary>
/// Перечисляет варианты turn state, которые используются в игровой логике вместо строковых значений
/// </summary>
public enum TurnState
{
    WaitingForRoll,
    RollingDice,
    MovingPlayer,
    ResolvingTile,
    TurnEnded
}
