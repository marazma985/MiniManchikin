using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Одна клетка на игровом поле, которая сообщает игре, когда игрок на нее попал
/// </summary>

public class BoardTile : MonoBehaviour
{
    /// <summary>
    /// Событие, которое передает игре клетку, на которую попал игрок
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
    /// Задает клетке ее номер на поле и игровой тип
    /// </summary>
    public void Configure(int newIndex, TileType newTileType)
    {
        index = Mathf.Max(0, newIndex);
        tileType = newTileType;
        ApplyName();
    }
    /// <summary>
    /// Запускает эффект клетки, когда игрок на нее попадает
    /// </summary>
    public void Enter()
    {
        OnEnter();
        entered.Invoke(this);
    }
    /// <summary>
    /// Вызывается, когда фишка игрока входит на эту клетку
    /// </summary>
    protected virtual void OnEnter()
    {
    }
    /// <summary>
    /// Подставляет индекс клетки из ее позиции в иерархии
    /// </summary>
    private void Reset()
    {
        ApplyName();
    }
    /// <summary>
    /// Обновляет имя клетки в иерархии после изменения индекса или типа
    /// </summary>
    private void OnValidate()
    {
        index = Mathf.Max(0, index);
        ApplyName();
    }
    /// <summary>
    /// Обновляет имя объекта клетки в иерархии по ее индексу и типу
    /// </summary>
    private void ApplyName()
    {
        gameObject.name = $"Tile_{index:00}";
    }
    /// <summary>
    /// Рисует подсказку клетки в окне сцены Unity
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
    /// Возвращает цвет gizmo для типа клетки в редакторе
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
