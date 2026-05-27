/// <summary>
/// Отвечает за завершение партии и экран результата, связанные с GameResultContext
/// </summary>
public static class GameResultContext
{
    public static GameResultType CurrentResult { get; private set; } = GameResultType.Lose;
    public static bool HasResult { get; private set; }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public static void SetResult(GameResultType result)
    {
        CurrentResult = result;
        HasResult = true;
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    public static void Clear()
    {
        CurrentResult = GameResultType.Lose;
        HasResult = false;
    }
}
