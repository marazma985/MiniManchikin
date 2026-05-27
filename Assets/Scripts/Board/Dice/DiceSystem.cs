using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Выдает случайное число от 1 до 6 и сообщает игре результат броска
/// </summary>

public sealed class DiceSystem : MonoBehaviour
{
    /// <summary>
    /// Событие, которое передает игре выпавшее число кубика
    /// </summary>
    [Serializable]
    public sealed class DiceRolledEvent : UnityEvent<int>
    {
    }

    [SerializeField] private DiceRolledEvent onDiceRolled = new DiceRolledEvent();

    public DiceRolledEvent OnDiceRolled => onDiceRolled;
    /// <summary>
    /// Бросает кубик от 1 до 6 и сообщает сцене выпавшее число
    /// </summary>
    public int Roll()
    {
        var value = UnityEngine.Random.Range(1, 7);
        onDiceRolled.Invoke(value);
        return value;
    }
}
