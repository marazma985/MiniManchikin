public static class GameResultContext
{
    public static GameResultType CurrentResult { get; private set; } = GameResultType.Lose;
    public static bool HasResult { get; private set; }

    public static void SetResult(GameResultType result)
    {
        CurrentResult = result;
        HasResult = true;
    }

    public static void Clear()
    {
        CurrentResult = GameResultType.Lose;
        HasResult = false;
    }
}
