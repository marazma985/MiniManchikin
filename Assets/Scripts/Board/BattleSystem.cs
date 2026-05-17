using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleSystem : MonoBehaviour
{
    private const string PlayerName = "\u041a\u0430\u0440\u0430\u043c\u0435\u043b\u044c\u043a\u0430";
    private const int EscapeSuccessRoll = 5;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private BattleModalView battleModalView;
    [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();
    [SerializeField, Min(0)] private int equipmentBonus;
    [SerializeField, Min(0)] private int cardBonus;
    [SerializeField, Min(0)] private int diceBonus;

    private BattleModalData currentBattleData;
    private EnemyData currentEnemy;
    private Action battleCompleted;
    private BattlePhase phase;
    private int currentBattleDiceBonus;
    private bool battleDiceUsed;

    public event Action BattleStateChanged;

    public bool IsBattleActive => currentBattleData != null;
    public bool CanUseBattleDice => IsBattleActive && phase == BattlePhase.WaitingForResolve && !battleDiceUsed && GetPowerDifference() >= 1 && GetPowerDifference() <= 6;

    private enum BattlePhase
    {
        None,
        WaitingForResolve,
        WaitingForEscapeRoll,
        WaitingForClose
    }

    public void StartBattle()
    {
        StartBattle(null);
    }

    public void StartBattle(Action onBattleCompleted)
    {
        if (IsBattleActive)
        {
            Debug.LogWarning("Battle is already active.");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning("BattleSystem requires PlayerStats.");
            onBattleCompleted?.Invoke();
            return;
        }

        if (diceSystem == null)
        {
            Debug.LogWarning("BattleSystem requires DiceSystem.");
            onBattleCompleted?.Invoke();
            return;
        }

        if (battleModalView == null)
        {
            Debug.LogWarning("BattleSystem requires BattleModalView.");
            onBattleCompleted?.Invoke();
            return;
        }

        var enemy = SelectRandomEnemy();
        if (enemy == null)
        {
            Debug.LogWarning("BattleSystem has no enemies to start battle.");
            onBattleCompleted?.Invoke();
            return;
        }

        battleCompleted = onBattleCompleted;
        currentEnemy = enemy;
        currentBattleDiceBonus = 0;
        battleDiceUsed = false;
        currentBattleData = CreateBattleData(enemy);
        phase = BattlePhase.WaitingForResolve;
        battleModalView.Show(currentBattleData);
        battleModalView.UpdateState(CanUseBattleDice ? "Battle dice available" : "Battle dice unavailable", "Resolve Battle");
        BattleStateChanged?.Invoke();
    }

    public bool RollBattleDice()
    {
        if (!IsBattleActive)
        {
            Debug.LogWarning("Battle dice requested, but there is no active battle.");
            return false;
        }

        if (!CanUseBattleDice)
        {
            Debug.LogWarning("Battle dice is unavailable.");
            return false;
        }

        currentBattleDiceBonus = diceSystem.Roll();
        battleDiceUsed = true;
        currentBattleData = CreateBattleData(currentEnemy);
        battleModalView.Show(currentBattleData);
        battleModalView.UpdateState($"Battle dice used: +{currentBattleDiceBonus}", "Resolve Battle");
        Debug.Log($"Battle dice used: +{currentBattleDiceBonus}. Player total is now {currentBattleData.PlayerTotalPower}.");
        BattleStateChanged?.Invoke();
        return true;
    }

    public void ResolveCurrentBattle()
    {
        if (currentBattleData == null)
            return;

        if (phase == BattlePhase.WaitingForEscapeRoll)
        {
            RollEscape();
            return;
        }

        if (phase == BattlePhase.WaitingForClose)
        {
            CompleteBattle();
            return;
        }

        if (phase != BattlePhase.WaitingForResolve)
            return;

        if (currentBattleData.PlayerTotalPower > currentBattleData.EnemyTotalPower)
        {
            playerStats.SetLevel(playerStats.Level + 1);
            Debug.Log($"Battle won: {currentBattleData.PlayerName} defeated {currentBattleData.EnemyName}. Level is now {playerStats.Level}.");
            battleModalView.UpdateState("Player won", "Close");
            phase = BattlePhase.WaitingForClose;
            BattleStateChanged?.Invoke();
        }
        else
        {
            Debug.Log($"Battle lost: {currentBattleData.PlayerName} failed against {currentBattleData.EnemyName}.");
            battleModalView.UpdateState("Player lost, trying to escape", "Roll Escape");
            phase = BattlePhase.WaitingForEscapeRoll;
            BattleStateChanged?.Invoke();
        }
    }

    private void OnEnable()
    {
        if (battleModalView != null)
            battleModalView.ResolveRequested += ResolveCurrentBattle;
    }

    private void OnDisable()
    {
        if (battleModalView != null)
            battleModalView.ResolveRequested -= ResolveCurrentBattle;
    }

    private void RollEscape()
    {
        var escapeRoll = diceSystem.Roll();
        var escapeBonus = GetEquipmentEffectBonus(EffectType.EscapeBonus);
        var finalEscapeValue = escapeRoll + escapeBonus;
        if (finalEscapeValue >= EscapeSuccessRoll)
        {
            Debug.Log($"Escape successful. Base roll: {escapeRoll}, escape bonus: {escapeBonus}, final escape value: {finalEscapeValue}.");
            battleModalView.UpdateState("Escape successful", "Close");
        }
        else
        {
            Debug.Log($"Escape failed. Base roll: {escapeRoll}, escape bonus: {escapeBonus}, final escape value: {finalEscapeValue}.");
            ApplyPenalty();
            battleModalView.UpdateState("Escape failed. Penalty applied", "Close");
        }

        phase = BattlePhase.WaitingForClose;
        BattleStateChanged?.Invoke();
    }

    private void ApplyPenalty()
    {
        if (currentEnemy == null || currentEnemy.PenaltyValue <= 0)
            return;

        switch (currentEnemy.PenaltyType)
        {
            case MonsterPenaltyType.LoseHp:
                if (playerInventory != null && playerInventory.TryBreakArmorForHpLoss())
                {
                    Debug.Log($"{currentEnemy.EnemyName} HP loss penalty was prevented by armor.");
                    break;
                }

                playerStats.TakeDamage(currentEnemy.PenaltyValue);
                Debug.Log($"{currentEnemy.EnemyName} penalty applied: lose {currentEnemy.PenaltyValue} HP.");
                break;
            case MonsterPenaltyType.LoseLevel:
                playerStats.SetLevel(playerStats.Level - currentEnemy.PenaltyValue);
                Debug.Log($"{currentEnemy.EnemyName} penalty applied: lose {currentEnemy.PenaltyValue} level.");
                break;
        }
    }

    private void CompleteBattle()
    {
        battleModalView.Hide();
        currentBattleData = null;
        currentEnemy = null;
        phase = BattlePhase.None;
        currentBattleDiceBonus = 0;
        battleDiceUsed = false;
        BattleStateChanged?.Invoke();

        var onCompleted = battleCompleted;
        battleCompleted = null;
        onCompleted?.Invoke();
    }

    private EnemyData SelectRandomEnemy()
    {
        if (enemies == null || enemies.Count == 0)
            return null;

        var validEnemies = new List<EnemyData>();
        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
                validEnemies.Add(enemies[i]);
        }

        if (validEnemies.Count == 0)
            return null;

        return validEnemies[UnityEngine.Random.Range(0, validEnemies.Count)];
    }

    private BattleModalData CreateBattleData(EnemyData enemy)
    {
        var totalEquipmentBonus = equipmentBonus + GetEquipmentEffectBonus(EffectType.Power);
        var playerEntries = new List<BattlePowerEntry>
        {
            new BattlePowerEntry("Level", playerStats.Level),
            new BattlePowerEntry("Equipment bonus", totalEquipmentBonus)
        };

        var totalDiceBonus = diceBonus + currentBattleDiceBonus;
        if (totalDiceBonus > 0)
            playerEntries.Add(new BattlePowerEntry("Dice bonus", totalDiceBonus));

        if (cardBonus > 0)
            playerEntries.Add(new BattlePowerEntry("Card bonuses", cardBonus));

        var enemyEntries = new List<BattlePowerEntry>
        {
            new BattlePowerEntry("Monster level", enemy.Level),
            new BattlePowerEntry("Monster bonuses", enemy.BonusPower)
        };

        return new BattleModalData
        {
            PlayerName = PlayerName,
            PlayerSprite = playerSprite,
            PlayerPowerEntries = playerEntries,
            PlayerTotalPower = SumEntries(playerEntries),
            EnemyName = enemy.EnemyName,
            EnemySprite = enemy.EnemySprite,
            EnemyPowerEntries = enemyEntries,
            EnemyTotalPower = SumEntries(enemyEntries)
        };
    }

    private static int SumEntries(IReadOnlyList<BattlePowerEntry> entries)
    {
        var total = 0;
        for (var i = 0; i < entries.Count; i++)
            total += entries[i].Value;

        return total;
    }

    private int GetPowerDifference()
    {
        return currentBattleData == null ? 0 : currentBattleData.EnemyTotalPower - currentBattleData.PlayerTotalPower;
    }

    private int GetEquipmentEffectBonus(EffectType effectType)
    {
        return playerInventory != null ? playerInventory.GetTotalEffectValue(effectType) : 0;
    }
}
