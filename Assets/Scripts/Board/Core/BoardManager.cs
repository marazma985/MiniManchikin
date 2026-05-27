using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Хранит маршрут игрового поля и текущую клетку, на которой стоит фишка игрока
/// </summary>

public sealed class BoardManager : MonoBehaviour
{
    [SerializeField] private List<BoardTile> tiles = new List<BoardTile>();
    [SerializeField] private bool cyclePath = true;
    [SerializeField, Min(0)] private int currentIndex;

    public IReadOnlyList<BoardTile> Tiles => tiles;
    public bool CyclePath => cyclePath;
    public int CurrentIndex => currentIndex;
    public BoardTile CurrentTile => GetTile(currentIndex);
    /// <summary>
    /// Возвращает клетку поля по ее индексу
    /// </summary>
    public BoardTile GetTile(int index)
    {
        if (tiles.Count == 0)
            return null;

        if (cyclePath)
            index = WrapIndex(index);

        if (index < 0 || index >= tiles.Count)
            return null;

        return tiles[index];
    }
    /// <summary>
    /// Возвращает следующую клетку после текущей позиции игрока
    /// </summary>
    public BoardTile GetNextTile()
    {
        return GetNextTile(currentIndex);
    }
    /// <summary>
    /// Возвращает следующую клетку после текущей позиции игрока
    /// </summary>
    public BoardTile GetNextTile(int fromIndex)
    {
        return GetTile(fromIndex + 1);
    }
    /// <summary>
    /// Передвигает текущий индекс поля на следующую клетку
    /// </summary>
    public BoardTile AdvanceToNextTile()
    {
        var nextIndex = currentIndex + 1;

        if (cyclePath)
            nextIndex = WrapIndex(nextIndex);
        else if (nextIndex >= tiles.Count)
            return null;

        currentIndex = nextIndex;
        return GetTile(currentIndex);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetCurrentIndex(int index)
    {
        currentIndex = cyclePath && tiles.Count > 0 ? WrapIndex(index) : Mathf.Max(0, index);
    }
    /// <summary>
    /// Ищет ближайшую впереди клетку нужного типа и возвращает расстояние до нее
    /// </summary>
    public bool TryGetForwardDistanceToNearestTileType(TileType tileType, out int steps)
    {
        steps = 0;

        if (tiles.Count == 0)
            return false;

        var maxSteps = cyclePath ? tiles.Count : tiles.Count - currentIndex - 1;
        for (var step = 1; step <= maxSteps; step++)
        {
            var tile = GetTile(currentIndex + step);
            if (tile == null)
                continue;

            if (tile.TileType != tileType)
                continue;

            steps = step;
            return true;
        }

        return false;
    }
    [ContextMenu("Collect Child Tiles")]
    /// <summary>
    /// Собирает клетки поля из дочерних объектов в сцене
    /// </summary>
    public void CollectChildTiles()
    {
        tiles.Clear();
        GetComponentsInChildren(true, tiles);
        SortTilesByIndex();
        ClampCurrentIndex();
    }
    [ContextMenu("Sort Tiles By Index")]
    /// <summary>
    /// Сортирует клетки поля по их игровому индексу
    /// </summary>
    public void SortTilesByIndex()
    {
        tiles.Sort((left, right) =>
        {
            if (left == null && right == null)
                return 0;
            if (left == null)
                return 1;
            if (right == null)
                return -1;

            return left.Index.CompareTo(right.Index);
        });
    }
    /// <summary>
    /// Заполняет удобные значения по умолчанию при добавлении компонента в Unity
    /// </summary>
    private void Reset()
    {
        CollectChildTiles();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        ClampCurrentIndex();
    }
    /// <summary>
    /// Заворачивает индекс клетки в границы игрового круга
    /// </summary>
    private int WrapIndex(int index)
    {
        var count = tiles.Count;
        return count == 0 ? 0 : (index % count + count) % count;
    }
    /// <summary>
    /// Ограничивает число допустимыми рамками
    /// </summary>
    private void ClampCurrentIndex()
    {
        if (tiles.Count == 0)
        {
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, tiles.Count - 1);
    }
}
