using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleSystem : MonoBehaviour
{
    private const string PlayerName = "\u041a\u0430\u0440\u0430\u043c\u0435\u043b\u044c\u043a\u0430";
    private const int EscapeSuccessRoll = 5;
    private const float TemporaryBattleStatusDuration = 2.5f;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private EventNotificationSystem eventNotificationSystem;
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private BattleModalView battleModalView;
    [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();
    [SerializeField, Min(0)] private int equipmentBonus;
    [SerializeField, Min(0)] private int cardBonus;
    [SerializeField, Min(0)] private int diceBonus;

    private BattleModalData currentBattleData;
    private EnemyData currentEnemy;
    private EnemyModifier currentEnemyModifier;
    private Action battleCompleted;
    private BattlePhase phase;
    private int currentBattleDiceBonus;
    private int temporaryCardPowerBonus;
    private int temporaryEscapeBonus;
    private bool battleDiceUsed;

    public event Action BattleStateChanged;

    public bool IsBattleActive => currentBattleData != null;
    public bool CanUseBattleDice => IsBattleActive && phase == BattlePhase.WaitingForResolve && !battleDiceUsed && GetPowerDifference() >= 0 && GetPowerDifference() <= 6;

    private enum BattlePhase
    {
        None,
        WaitingForResolve,
        WaitingForEscapeRoll,
        WaitingForReward,
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
        currentEnemyModifier = SelectRandomModifier(enemy);
        currentBattleDiceBonus = 0;
        temporaryCardPowerBonus = 0;
        temporaryEscapeBonus = 0;
        battleDiceUsed = false;
        currentBattleData = CreateBattleData(enemy);
        phase = BattlePhase.WaitingForResolve;
        battleModalView.Show(currentBattleData);
        battleModalView.ClearStatus();
        RefreshActionButton();
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
        RefreshActionButton();
        battleModalView.ShowTemporaryStatus($"Сила кубика: +{currentBattleDiceBonus}", TemporaryBattleStatusDuration);
        Debug.Log($"Battle dice used: +{currentBattleDiceBonus}. Player total is now {currentBattleData.PlayerTotalPower}.");
        BattleStateChanged?.Invoke();
        return true;
    }

    public bool AddTemporaryCardPower(int value)
    {
        if (!IsBattleActive || value <= 0)
        {
            Debug.LogWarning("Temporary card power can only be added during an active battle.");
            return false;
        }

        temporaryCardPowerBonus += value;
        RefreshBattleModal($"Бонус силы от карты: +{value}");
        Debug.Log($"Temporary card power bonus added: +{value}. Total temporary card power: {temporaryCardPowerBonus}.");
        BattleStateChanged?.Invoke();
        return true;
    }

    public bool AddTemporaryEscapeBonus(int value)
    {
        if (!IsBattleActive || value <= 0)
        {
            Debug.LogWarning("Temporary escape bonus can only be added during an active battle.");
            return false;
        }

        temporaryEscapeBonus += value;
        RefreshBattleModal($"Бонус побега от карты: +{value}");
        Debug.Log($"Temporary escape bonus added: +{value}. Total temporary escape bonus: {temporaryEscapeBonus}.");
        BattleStateChanged?.Invoke();
        return true;
    }

    public void RefreshCurrentBattleView(string status)
    {
        RefreshBattleModal(status);
        BattleStateChanged?.Invoke();
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

        if (phase == BattlePhase.WaitingForReward)
        {
            Debug.LogWarning("Choose a reward to complete the battle.");
            return;
        }

        if (phase != BattlePhase.WaitingForResolve)
            return;

        if (currentBattleData.PlayerTotalPower > currentBattleData.EnemyTotalPower)
            ResolveVictory();
        else
            RollEscape();
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
        var escapeBonus = GetEquipmentEffectBonus(EffectType.EscapeBonus) + temporaryEscapeBonus;
        var finalEscapeValue = escapeRoll + escapeBonus;
        if (finalEscapeValue >= EscapeSuccessRoll)
        {
            Debug.Log($"Escape successful. Base roll: {escapeRoll}, escape bonus: {escapeBonus}, final escape value: {finalEscapeValue}.");
            battleModalView.ShowPersistentStatus("Побег удался");
        }
        else
        {
            Debug.Log($"Escape failed. Base roll: {escapeRoll}, escape bonus: {escapeBonus}, final escape value: {finalEscapeValue}.");
            ApplyPenalty();
            battleModalView.ShowPersistentStatus("Побег не удался, штраф применён");
        }

        phase = BattlePhase.WaitingForClose;
        RefreshActionButton();
        BattleStateChanged?.Invoke();
    }

    private void ResolveVictory()
    {
        var previousLevel = playerStats.Level;
        playerStats.SetLevel(playerStats.Level + 1);
        NotifyEffect(EffectType.Level, playerStats.Level - previousLevel, EffectNotificationStatus.Success);
        Debug.Log($"Battle won: {currentBattleData.PlayerName} defeated {currentBattleData.EnemyName}. Level is now {playerStats.Level}.");

        if (rewardSystem != null && rewardSystem.ShowBattleRewards(HandleRewardAccepted))
        {
            battleModalView.Hide();
            phase = BattlePhase.WaitingForReward;
            BattleStateChanged?.Invoke();
            return;
        }

        CompleteBattle();
    }

    private void ApplyPenalty()
    {
        if (currentEnemy == null)
            return;

        var penaltyEffects = currentEnemy.PenaltyEffects;
        if (penaltyEffects == null || penaltyEffects.Count == 0)
            return;

        for (var i = 0; i < penaltyEffects.Count; i++)
            ApplyPenaltyEffect(penaltyEffects[i]);
    }

    private void ApplyPenaltyEffect(EffectData effect)
    {
        if (effect == null)
        {
            Debug.LogWarning($"{currentEnemy.EnemyName} has a missing penalty effect.");
            return;
        }

        switch (effect.EffectType)
        {
            case EffectType.HpRestore:
                ApplyHpPenalty(effect.Value);
                break;
            case EffectType.Level:
                ApplyLevelPenalty(effect.Value);
                break;
            case EffectType.RemoveCard:
                ApplyRemoveCardPenalty(effect.Value);
                break;
            default:
                Debug.LogWarning($"{currentEnemy.EnemyName} has unsupported penalty effect '{effect.EffectType}'.");
                break;
        }
    }

    private void ApplyHpPenalty(int value)
    {
        if (value >= 0)
        {
            Debug.LogWarning($"{currentEnemy.EnemyName} HP penalty requires negative HpRestore value.");
            return;
        }

        var damage = -value;
        if (playerInventory != null && playerInventory.TryBreakArmorForHpLoss())
        {
            NotifyEffect(EffectType.HpRestore, value, EffectNotificationStatus.Blocked);
            Debug.Log($"{currentEnemy.EnemyName} HP loss penalty was prevented by armor.");
            return;
        }

        var previousHp = playerStats.CurrentHp;
        playerStats.TakeDamage(damage);
        var lostHp = previousHp - playerStats.CurrentHp;
        NotifyEffect(EffectType.HpRestore, lostHp > 0 ? -lostHp : value, lostHp > 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect);
        Debug.Log($"{currentEnemy.EnemyName} penalty applied: lose {damage} HP.");
    }

    private void ApplyLevelPenalty(int value)
    {
        if (value >= 0)
        {
            Debug.LogWarning($"{currentEnemy.EnemyName} level penalty requires negative Level value.");
            return;
        }

        var levelLoss = -value;
        var previousLevel = playerStats.Level;
        playerStats.SetLevel(playerStats.Level + value);
        var levelChange = playerStats.Level - previousLevel;
        NotifyEffect(EffectType.Level, levelChange != 0 ? levelChange : value, levelChange != 0 ? EffectNotificationStatus.Success : EffectNotificationStatus.NoEffect);
        Debug.Log($"{currentEnemy.EnemyName} penalty applied: lose {levelLoss} level.");
    }

    private void ApplyRemoveCardPenalty(int value)
    {
        if (cardSystem == null)
        {
            Debug.LogWarning($"{currentEnemy.EnemyName} cannot remove a card because CardSystem is not assigned.");
            return;
        }

        var removeCount = Mathf.Max(1, value);
        for (var i = 0; i < removeCount; i++)
        {
            if (cardSystem.RemoveRandomCard(Rarity.Common))
            {
                Debug.Log($"{currentEnemy.EnemyName} penalty applied: remove random common card.");
                continue;
            }

            NotifyEffect(EffectType.RemoveCard, removeCount, EffectNotificationStatus.Failed);
            Debug.LogWarning($"{currentEnemy.EnemyName} penalty could not remove a common card.");
            return;
        }

        NotifyEffect(EffectType.RemoveCard, removeCount, EffectNotificationStatus.Success);
    }

    private EnemyModifier SelectRandomModifier(EnemyData enemy)
    {
        if (enemy == null || enemy.Modifiers == null || enemy.Modifiers.Count == 0)
            return null;

        var validModifiers = new List<EnemyModifier>();
        for (var i = 0; i < enemy.Modifiers.Count; i++)
        {
            if (enemy.Modifiers[i] != null)
                validModifiers.Add(enemy.Modifiers[i]);
        }

        if (validModifiers.Count == 0)
            return null;

        var selectedIndex = UnityEngine.Random.Range(0, validModifiers.Count + 1);
        if (selectedIndex == 0)
            return null;

        var modifier = validModifiers[selectedIndex - 1];
        Debug.Log($"{enemy.EnemyName} modifier selected: {modifier.ModifierName}.");
        return modifier;
    }

    private static int GetModifierPower(EnemyModifier modifier)
    {
        if (modifier == null || modifier.Effects == null)
            return 0;

        var power = 0;
        for (var i = 0; i < modifier.Effects.Count; i++)
        {
            var effect = modifier.Effects[i];
            if (effect != null && effect.EffectType == EffectType.Power)
                power += effect.Value;
        }

        return power;
    }

    private void CompleteBattle()
    {
        battleModalView.Hide();
        currentBattleData = null;
        currentEnemy = null;
        currentEnemyModifier = null;
        phase = BattlePhase.None;
        currentBattleDiceBonus = 0;
        temporaryCardPowerBonus = 0;
        temporaryEscapeBonus = 0;
        battleDiceUsed = false;
        BattleStateChanged?.Invoke();

        var onCompleted = battleCompleted;
        battleCompleted = null;
        onCompleted?.Invoke();
    }

    private void HandleRewardAccepted()
    {
        Debug.Log("Battle reward accepted. Completing battle.");
        CompleteBattle();
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
            new BattlePowerEntry("Уровень", playerStats.Level),
            new BattlePowerEntry("Бонус экипировки", totalEquipmentBonus)
        };

        var totalDiceBonus = diceBonus + currentBattleDiceBonus;
        if (totalDiceBonus > 0)
            playerEntries.Add(new BattlePowerEntry("Бонус кубика", totalDiceBonus));

        var totalCardBonus = cardBonus + temporaryCardPowerBonus;
        if (totalCardBonus > 0)
            playerEntries.Add(new BattlePowerEntry("Бонус карт", totalCardBonus));

        var enemyEntries = new List<BattlePowerEntry>
        {
            new BattlePowerEntry("Уровень монстра", enemy.BaseLevel)
        };

        var modifierPower = GetModifierPower(currentEnemyModifier);
        if (currentEnemyModifier != null)
        {
            var modifierName = string.IsNullOrEmpty(currentEnemyModifier.ModifierName)
                ? "Модификатор"
                : $"Модификатор: {currentEnemyModifier.ModifierName}";
            enemyEntries.Add(new BattlePowerEntry(modifierName, modifierPower));
        }

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

    private void RefreshBattleModal(string status)
    {
        if (currentEnemy == null || battleModalView == null)
            return;

        currentBattleData = CreateBattleData(currentEnemy);
        battleModalView.Show(currentBattleData);
        RefreshActionButton();

        if (!string.IsNullOrEmpty(status))
            battleModalView.ShowTemporaryStatus(status, TemporaryBattleStatusDuration);
    }

    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
    }

    private string GetCurrentActionButtonText()
    {
        switch (phase)
        {
            case BattlePhase.WaitingForEscapeRoll:
                return "Пытаться сбежать";
            case BattlePhase.WaitingForReward:
                return string.Empty;
            case BattlePhase.WaitingForClose:
                return "Закрыть";
            case BattlePhase.WaitingForResolve:
                return currentBattleData != null && currentBattleData.PlayerTotalPower > currentBattleData.EnemyTotalPower
                    ? "Победа"
                    : "Пытаться сбежать";
            default:
                return string.Empty;
        }
    }

    private void RefreshActionButton()
    {
        if (battleModalView != null)
            battleModalView.SetActionButtonText(GetCurrentActionButtonText());
    }
}
