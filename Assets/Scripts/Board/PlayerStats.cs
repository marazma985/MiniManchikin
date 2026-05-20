using System;
using UnityEngine;

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

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        SetHp(currentHp + amount);
    }

    public void SetLevel(int newLevel)
    {
        newLevel = Mathf.Max(1, newLevel);
        if (level == newLevel)
            return;

        level = newLevel;
        NotifyLevelChanged();
    }

    [ContextMenu("Test Take 1 Damage")]
    private void TestTakeOneDamage()
    {
        TakeDamage(1);
    }

    [ContextMenu("Test Heal 1")]
    private void TestHealOne()
    {
        Heal(1);
    }

    [ContextMenu("Test Set Level 10")]
    private void TestSetLevelTen()
    {
        SetLevel(10);
    }

    [ContextMenu("Test Take Lethal Damage")]
    private void TestTakeLethalDamage()
    {
        TakeDamage(maxHp);
    }

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        CacheNotifiedValues();
    }

    private void OnValidate()
    {
        maxHp = Mathf.Max(1, maxHp);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        level = Mathf.Max(1, level);

        if (Application.isPlaying)
            NotifyRuntimeInspectorChanges();
    }

    private void SetHp(int newHp)
    {
        newHp = Mathf.Clamp(newHp, 0, maxHp);
        if (currentHp == newHp)
            return;

        currentHp = newHp;
        NotifyHpChanged();
    }

    private void NotifyRuntimeInspectorChanges()
    {
        if (lastNotifiedHp != currentHp || lastNotifiedMaxHp != maxHp)
            NotifyHpChanged();

        if (lastNotifiedLevel != level)
            NotifyLevelChanged();
    }

    private void NotifyHpChanged()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    private void NotifyLevelChanged()
    {
        lastNotifiedLevel = level;
        OnLevelChanged?.Invoke(level);
    }

    private void CacheNotifiedValues()
    {
        lastNotifiedHp = currentHp;
        lastNotifiedMaxHp = maxHp;
        lastNotifiedLevel = level;
    }
}
