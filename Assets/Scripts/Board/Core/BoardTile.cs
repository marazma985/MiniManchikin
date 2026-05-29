using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Базовый и абстрактный класс всех клеток
/// Наследует им методы и некоторые с возможностью переопределения (полиморфизм)
/// </summary>
public class BoardTile : MonoBehaviour, IBoardTile
{
    //Свойства доступные для установки из инспектора в юнити
    
    //класс события который работает с клеткой
    [Serializable]
    public sealed class BoardTileEvent : UnityEvent<BoardTile>
    {
    }

    [SerializeField, Min(0)] private int index;
    //Свойство которое хранит событие 
    [SerializeField] private BoardTileEvent entered = new BoardTileEvent();

    //Сокращение get конструкции свойства, без set
    public int Index => index;
    public virtual TileType Type => TileType.None;
    public BoardTileEvent Entered => entered;

    //Функция которая вызывает событие то, что на клетку встали
    //Настроена, но не применяется на текущем моменте (выводило в консоль информацию о клетке для дебага)
    //Можно будет сделать подсветку при наступании
    public void Enter()
    {
        OnEnter();
        entered.Invoke(this);
    }

    /// <summary>
    /// Основная функция выполнения действия клетки
    /// </summary>
    /// <param name="context"></param>
    /// <param name="onResolved"></param>
    public virtual void Resolve(TileResolutionContext context, Action onResolved)
    {
        onResolved?.Invoke();
    }

    /// Новая функция, появилась для карт которые отправляют игрока на клетку
    /// теперь клетка сама скажет, подходит ли она под нужный тип
    /// <summary>
    /// Определяет подходит ли клетка под нужный тип
    /// </summary>
    /// <param name="targetTileType"></param>
    /// <returns></returns>
    public bool Matches(TileType targetTileType)
    {
        return targetTileType != TileType.None && Type == targetTileType;
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
