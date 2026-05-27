using System;
using UnityEngine;
/// <summary>
/// Хранит здоровье и уровень игрока, от которых зависят бой, победа и поражение
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
    /// Отнимает здоровье у игрока и проверяет поражение
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp - amount);
    }
    /// <summary>
    /// Лечит игрока, не превышая максимальное здоровье
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp + amount);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
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
    /// Возвращает данные из сохранения или ранее запомненного состояния
    /// </summary>
    public void RestoreState(int restoredHp, int restoredLevel)
    {
        level = Mathf.Max(1, restoredLevel);
        currentHp = Mathf.Clamp(restoredHp, 0, maxHp);
        NotifyHpChanged();
        NotifyLevelChanged();
    }
    /// <summary>
    /// Выставляет стартовое здоровье и уровень игрока
    /// </summary>
    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        CacheNotifiedValues();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
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
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
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
    /// Обновляет события здоровья и уровня после правок в инспекторе
    /// </summary>
    private void NotifyRuntimeInspectorChanges()
    {
        if (lastNotifiedHp != currentHp || lastNotifiedMaxHp != maxHp)
            NotifyHpChanged();

        if (lastNotifiedLevel != level)
            NotifyLevelChanged();
    }
    /// <summary>
    /// Сообщает HUD и игровым системам, что здоровье игрока изменилось
    /// </summary>
    private void NotifyHpChanged()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        OnHpChanged?.Invoke(currentHp, maxHp);
    }
    /// <summary>
    /// Сообщает HUD и игровым системам, что уровень игрока изменился
    /// </summary>
    private void NotifyLevelChanged()
    {
        lastNotifiedLevel = level;
        OnLevelChanged?.Invoke(level);
    }
    /// <summary>
    /// Запоминает текущее значение для дальнейшего сравнения
    /// </summary>
    private void CacheNotifiedValues()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        lastNotifiedLevel = level;
    }
}
