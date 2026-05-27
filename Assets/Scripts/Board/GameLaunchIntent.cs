public enum GameLaunchMode
{
    NewGame,
    Continue
}

public static class GameLaunchIntent
{
    private static GameLaunchMode mode = GameLaunchMode.NewGame;

    public static GameLaunchMode Mode => mode;

    public static void Set(GameLaunchMode launchMode)
    {
        mode = launchMode;
    }

    public static GameLaunchMode Consume()
    {
        var launchMode = mode;
        mode = GameLaunchMode.NewGame;
        return launchMode;
    }
}
