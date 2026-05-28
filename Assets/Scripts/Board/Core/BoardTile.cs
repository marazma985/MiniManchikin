using System;
using UnityEngine;
using UnityEngine.Events;

public class BoardTile : MonoBehaviour, IBoardTile
{
    [Serializable]
    public sealed class BoardTileEvent : UnityEvent<BoardTile>
    {
    }

    [SerializeField, Min(0)] private int index;
    [SerializeField] private BoardTileEvent entered = new BoardTileEvent();

    public int Index => index;
    public BoardTileEvent Entered => entered;

    public void Enter()
    {
        OnEnter();
        entered.Invoke(this);
    }

    public virtual void Resolve(TileResolutionContext context, Action onResolved)
    {
        onResolved?.Invoke();
    }

    public virtual bool Matches(TileTargetQuery query)
    {
        return query != null && query.Matches(this);
    }

    public virtual bool MatchesTargetId(string targetTileId)
    {
        return false;
    }

    protected virtual void OnEnter()
    {
    }

    private void Reset()
    {
        ApplyName();
    }

    private void OnValidate()
    {
        index = Mathf.Max(0, index);
        ApplyName();
    }

    private void ApplyName()
    {
        gameObject.name = $"Tile_{index:00}";
    }
}
