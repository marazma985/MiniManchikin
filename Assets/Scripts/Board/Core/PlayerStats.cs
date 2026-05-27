using System;
using UnityEngine;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с PlayerStats
/// </summary>

public sealed class PlayerStats : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHp = 5;
    [SerializeField, Min(0)] private int currentHp = 5;
    [SerializeField, Min(1)] private int level = 1;

    public event Action<int, int> OnHpChanged;
    public event Action<int> OnLevelChanged;

    private int lastNotifiedHp = -1;
    private int lastNotifiedMaxHp = -1;
    private int lastNotifiedLevel = -1;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int Level => level;
    /// <summary>
    /// Выполняет вспомогательную часть логики метода TakeDamage
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp - amount);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода Heal
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp + amount);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetLevel(int newLevel)
    {
        newLevel = Mathf.Max(1, newLevel);
        if (level == newLevel)
            return;

        level = newLevel;
        NotifyLevelChanged();
    }
    /// <summary>
    /// Восстанавливает состояние из сохраненных данных
    /// </summary>
    public void RestoreState(int restoredHp, int restoredLevel)
    {
        level = Mathf.Max(1, restoredLevel);
        currentHp = Mathf.Clamp(restoredHp, 0, maxHp);
        NotifyHpChanged();
        NotifyLevelChanged();
    }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        CacheNotifiedValues();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        maxHp = Mathf.Max(1, maxHp);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        level = Mathf.Max(1, level);

        if (Application.isPlaying)
            NotifyRuntimeInspectorChanges();
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private void SetHp(int newHp)
    {
        newHp = Mathf.Clamp(newHp, 0, maxHp);
        if (currentHp == newHp)
            return;

        currentHp = newHp;
        NotifyHpChanged();
    }
    /// <summary>
    /// Сообщает подписчикам, что состояние изменилось
    /// </summary>
    private void NotifyRuntimeInspectorChanges()
    {
        if (lastNotifiedHp != currentHp || lastNotifiedMaxHp != maxHp)
            NotifyHpChanged();

        if (lastNotifiedLevel != level)
            NotifyLevelChanged();
    }
    /// <summary>
    /// Сообщает подписчикам, что состояние изменилось
    /// </summary>
    private void NotifyHpChanged()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        OnHpChanged?.Invoke(currentHp, maxHp);
    }
    /// <summary>
    /// Сообщает подписчикам, что состояние изменилось
    /// </summary>
    private void NotifyLevelChanged()
    {
        lastNotifiedLevel = level;
        OnLevelChanged?.Invoke(level);
    }
    /// <summary>
    /// Сохраняет текущие значения, чтобы позже обнаружить изменения
    /// </summary>
    private void CacheNotifiedValues()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        lastNotifiedLevel = level;
    }
}
