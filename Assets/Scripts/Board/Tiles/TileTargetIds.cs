using System;

public static class TileTargetIds
{
    public const string RandomEvent = nameof(RandomEvent);
    public const string RareEvent = nameof(RareEvent);
    public const string Battle = nameof(Battle);
    public const string Buff = nameof(Buff);
    public const string Debuff = nameof(Debuff);

    public static bool Matches(string actualTargetId, string expectedTargetId)
    {
        return !string.IsNullOrWhiteSpace(actualTargetId)
            && string.Equals(actualTargetId, expectedTargetId, StringComparison.Ordinal);
    }
}
