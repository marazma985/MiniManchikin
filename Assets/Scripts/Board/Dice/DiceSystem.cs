using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Отвечает за механику или визуал кубика, связанные с DiceSystem
/// </summary>

public sealed class DiceSystem : MonoBehaviour
{
    /// <summary>
    /// Отвечает за механику или визуал кубика, связанные с DiceRolledEvent
    /// </summary>
    [Serializable]
    public sealed class DiceRolledEvent : UnityEvent<int>
    {
    }

    [SerializeField] private DiceRolledEvent onDiceRolled = new DiceRolledEvent();

    public DiceRolledEvent OnDiceRolled => onDiceRolled;
    /// <summary>
    /// Выполняет бросок кубика или запускает связанную с ним логику
    /// </summary>
    public int Roll()
    {
        var value = UnityEngine.Random.Range(1, 7);
        onDiceRolled.Invoke(value);
        return value;
    }
}
