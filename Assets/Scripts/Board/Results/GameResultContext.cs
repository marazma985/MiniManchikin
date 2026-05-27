/// <summary>
/// Передает результат партии из игровой сцены на финальный экран
/// </summary>
public static class GameResultContext
{
    public static GameResultType CurrentResult { get; private set; } = GameResultType.Lose;
    public static bool HasResult { get; private set; }
    /// <summary>
    /// Запоминает итог партии перед переходом на финальную сцену
    /// </summary>
    public static void SetResult(GameResultType result)
    {
        CurrentResult = result;
        HasResult = true;
    }
    /// <summary>
    /// Очищает данные результата после выхода с финального экрана
    /// </summary>
    public static void Clear()
    {
        CurrentResult = GameResultType.Lose;
        HasResult = false;
    }
}
