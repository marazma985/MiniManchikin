using System;

public sealed class TileTargetQuery
{
    public TileTargetQuery(Type targetTileClass)
        : this(targetTileClass, string.Empty)
    {
    }

    private TileTargetQuery(Type targetTileClass, string targetTileId)
    {
        if (targetTileClass != null && !typeof(BoardTile).IsAssignableFrom(targetTileClass))
            throw new ArgumentException("Target tile class must inherit from BoardTile.", nameof(targetTileClass));

        TargetTileClass = targetTileClass;
        TargetTileId = targetTileId;
    }

    public Type TargetTileClass { get; }
    public string TargetTileId { get; }
    public bool IsValid => TargetTileClass != null || !string.IsNullOrWhiteSpace(TargetTileId);
    public string Description => !string.IsNullOrWhiteSpace(TargetTileId) ? TargetTileId : TargetTileClass?.Name;

    public static TileTargetQuery For<TTile>() where TTile : BoardTile
    {
        return new TileTargetQuery(typeof(TTile));
    }

    public static TileTargetQuery ForId(string targetTileId)
    {
        return new TileTargetQuery(null, targetTileId);
    }

    public bool Matches(BoardTile tile)
    {
        if (tile == null)
            return false;

        if (!string.IsNullOrWhiteSpace(TargetTileId) && tile.MatchesTargetId(TargetTileId))
            return true;

        return TargetTileClass != null && TargetTileClass.IsInstanceOfType(tile);
    }
}
