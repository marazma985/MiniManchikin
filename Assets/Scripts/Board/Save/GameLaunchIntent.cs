/// <summary>
/// Режим запуска игровой сцены: начать заново или загрузить сохранение
/// </summary>
public enum GameLaunchMode
{
    NewGame,
    Continue
}
/// <summary>
/// Запоминает, как игрок входит в сцену поля: новая игра или продолжение
/// </summary>
public static class GameLaunchIntent
{
    private static GameLaunchMode mode = GameLaunchMode.NewGame;

    public static GameLaunchMode Mode => mode;
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public static void Set(GameLaunchMode launchMode)
    {
        mode = launchMode;
    }
    /// <summary>
    /// Возвращает выбранный режим запуска игры и сразу сбрасывает его
    /// </summary>
    public static GameLaunchMode Consume()
    {
        var launchMode = mode;
        mode = GameLaunchMode.NewGame;
        return launchMode;
    }
}
