/// <summary>
/// Перечисляет варианты game launch mode, которые используются в игровой логике вместо строковых значений
/// </summary>
public enum GameLaunchMode
{
    NewGame,
    Continue
}
/// <summary>
/// Отвечает за сохранение или восстановление данных партии, связанное с GameLaunchIntent
/// </summary>
public static class GameLaunchIntent
{
    private static GameLaunchMode mode = GameLaunchMode.NewGame;

    public static GameLaunchMode Mode => mode;
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public static void Set(GameLaunchMode launchMode)
    {
        mode = launchMode;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода Consume
    /// </summary>
    public static GameLaunchMode Consume()
    {
        var launchMode = mode;
        mode = GameLaunchMode.NewGame;
        return launchMode;
    }
}
