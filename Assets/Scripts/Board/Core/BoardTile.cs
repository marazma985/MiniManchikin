using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с BoardTile
/// </summary>

public class BoardTile : MonoBehaviour
{
    /// <summary>
    /// Отвечает за базовую механику игрового поля, связанную с BoardTileEvent
    /// </summary>
    [Serializable]
    public sealed class BoardTileEvent : UnityEvent<BoardTile>
    {
    }

    [SerializeField, Min(0)] private int index;
    [SerializeField] private TileType tileType;
    [SerializeField] private BoardTileEvent entered = new BoardTileEvent();

    public int Index => index;
    public TileType TileType => tileType;
    public BoardTileEvent Entered => entered;
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    public void Configure(int newIndex, TileType newTileType)
    {
        index = Mathf.Max(0, newIndex);
        tileType = newTileType;
        ApplyName();
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода Enter
    /// </summary>
    public void Enter()
    {
        OnEnter();
        entered.Invoke(this);
    }
    /// <summary>
    /// Реагирует на событие enter
    /// </summary>
    protected virtual void OnEnter()
    {
    }
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        ApplyName();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        index = Mathf.Max(0, index);
        ApplyName();
    }
    /// <summary>
    /// Применяет изменение к игровому или визуальному состоянию
    /// </summary>
    private void ApplyName()
    {
        gameObject.name = $"Tile_{index:00}";
    }
    /// <summary>
    /// Реагирует на событие draw gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        var color = GetGizmoColor(tileType);
        color.a = 0.35f;
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(1.8f, 0.8f, 0.05f));

        color.a = 1f;
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, new Vector3(1.8f, 0.8f, 0.05f));
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private static Color GetGizmoColor(TileType type)
    {
        switch (type)
        {
            case TileType.RandomEvent:
                return new Color(1f, 0.75f, 0.2f);
            case TileType.Debuff:
                return new Color(0.55f, 0.2f, 0.9f);
            case TileType.Battle:
                return new Color(1f, 0.25f, 0.2f);
            case TileType.RareEvent:
                return new Color(0.2f, 0.75f, 1f);
            case TileType.Buff:
                return new Color(0.25f, 0.85f, 0.35f);
            default:
                return Color.white;
        }
    }
}
