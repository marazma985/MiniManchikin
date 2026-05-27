using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Главная логика боя: выбирает врага, считает силы, дает шанс на кубик, обрабатывает побег, штрафы и награды
/// </summary>

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
    [SerializeField, Min(0)] private int enemyBalanceLowerOffset = 5;
    [SerializeField, Min(0)] private int enemyBalanceUpperOffset = 5;
    [SerializeField, Min(0)] private int equipmentBonus;
    [SerializeField, Min(0)] private int cardBonus;
    [SerializeField, Min(0)] private int diceBonus;

    private BattleModalData currentBattleData;
    private EnemyData currentEnemy;
    private readonly List<EnemyModifier> currentEnemyModifiers = new List<EnemyModifier>();
    private Action battleCompleted;
    private BattlePhase phase;
    private int currentBattleDiceBonus;
    private int temporaryCardPowerBonus;
    private int temporaryEscapeBonus;
    private bool battleDiceUsed;
    private bool escapeRollInProgress;
    private bool hasPendingBattleDice;
    private int pendingBattleDiceValue;
    private bool hasPendingEscapeRoll;
    private int pendingEscapeRollValue;

    public event Action BattleStateChanged;

    public bool IsBattleActive => currentBattleData != null;
    public bool CanUseBattleDice => IsBattleActive && phase == BattlePhase.WaitingForResolve && !battleDiceUsed && !escapeRollInProgress && GetPowerDifference() >= 0 && GetPowerDifference() < 6;
    /// <summary>
    /// Текущий этап боя: побег, награда, закрытие или обычное решение исхода
    /// </summary>
    private enum BattlePhase
    {
        None,
        WaitingForResolve,
        WaitingForEscapeRoll,
        WaitingForReward,
        WaitingForClose
    }
    /// <summary>
    /// Начинает бой и подготавливает все, что игрок увидит в окне боя
    /// </summary>
    public void StartBattle()
    {
        StartBattle(null);
    }
    /// <summary>
    /// Начинает бой и подготавливает все, что игрок увидит в окне боя
    /// </summary>
    public void StartBattle(Action onBattleCompleted)
    {
        // Проверки в начале не дают случайно запустить второй бой поверх уже открытого
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

        // После этой строки бой уже начат, поэтому все случайные результаты сразу запоминаются
        battleCompleted = onBattleCompleted;
        currentEnemy = enemy;
        SelectRandomModifiers(enemy, currentEnemyModifiers);
        currentBattleDiceBonus = 0;
        temporaryCardPowerBonus = 0;
        temporaryEscapeBonus = 0;
        battleDiceUsed = false;
        escapeRollInProgress = false;
        hasPendingBattleDice = false;
        pendingBattleDiceValue = 0;
        hasPendingEscapeRoll = false;
        pendingEscapeRollValue = 0;
        currentBattleData = CreateBattleData(enemy);
        phase = BattlePhase.WaitingForResolve;
        battleModalView.Show(currentBattleData);
        battleModalView.ClearStatus();
        LogBattleOpened(enemy);
        RefreshActionButton();
        BattleStateChanged?.Invoke();
    }
    /// <summary>
    /// Тестовая кнопка в инспекторе для быстрого открытия случайного боя
    /// </summary>
    public void TestStartRandomBattle()
    {
        StartBattle();
    }
    /// <summary>
    /// Закрывает тестовый бой без награды и штрафа
    /// </summary>
    public void CloseBattleWithoutConsequences()
    {
        if (!IsBattleActive)
        {
            Debug.Log("No active battle to close.");
            return;
        }

        Debug.Log("Battle closed without rewards or penalties.");
        CompleteBattle();
    }
    /// <summary>
    /// Собирает важные данные текущего состояния для файла сохранения
    /// </summary>
    public BattleSaveData CaptureSaveData()
    {
        if (!IsBattleActive)
            return null;

        return new BattleSaveData
        {
            active = true,
            enemyId = currentEnemy != null ? currentEnemy.EnemyId : string.Empty,
            modifierIndexes = GetCurrentModifierIndexes(),
            phase = (int)phase,
            currentBattleDiceBonus = currentBattleDiceBonus,
            temporaryCardPowerBonus = temporaryCardPowerBonus,
            temporaryEscapeBonus = temporaryEscapeBonus,
            battleDiceUsed = battleDiceUsed,
            hasPendingBattleDice = hasPendingBattleDice,
            pendingBattleDiceValue = pendingBattleDiceValue,
            hasPendingEscapeRoll = hasPendingEscapeRoll,
            pendingEscapeRollValue = pendingEscapeRollValue
        };
    }
    /// <summary>
    /// Возвращает состояние игры из сохранения без новых случайных результатов
    /// </summary>
    public void RestoreFromSave(
        BattleSaveData saveData,
        GameSaveContentResolver resolver,
        RewardSystem restoredRewardSystem,
        IReadOnlyList<RewardSaveData> rewardOptions,
        Action onBattleCompleted)
    {
        // При восстановлении используются сохраненные id и индексы, а не новый случайный выбор
        if (saveData == null || !saveData.active || resolver == null)
            return;

        var enemy = resolver.GetEnemy(saveData.enemyId);
        if (enemy == null)
        {
            Debug.LogWarning($"Could not restore battle enemy '{saveData.enemyId}'.");
            onBattleCompleted?.Invoke();
            return;
        }

        battleCompleted = onBattleCompleted;
        currentEnemy = enemy;
        currentEnemyModifiers.Clear();
        RestoreModifierIndexes(enemy, saveData.modifierIndexes);
        currentBattleDiceBonus = saveData.currentBattleDiceBonus;
        temporaryCardPowerBonus = saveData.temporaryCardPowerBonus;
        temporaryEscapeBonus = saveData.temporaryEscapeBonus;
        battleDiceUsed = saveData.battleDiceUsed;
        hasPendingBattleDice = saveData.hasPendingBattleDice;
        pendingBattleDiceValue = saveData.pendingBattleDiceValue;
        hasPendingEscapeRoll = saveData.hasPendingEscapeRoll;
        pendingEscapeRollValue = saveData.pendingEscapeRollValue;
        escapeRollInProgress = saveData.hasPendingEscapeRoll;
        phase = (BattlePhase)Mathf.Clamp(saveData.phase, (int)BattlePhase.WaitingForResolve, (int)BattlePhase.WaitingForClose);
        currentBattleData = CreateBattleData(enemy);

        // Бой может загрузиться либо как само окно боя, либо как уже открытый выбор награды
        if (phase == BattlePhase.WaitingForReward)
        {
            battleModalView.Hide();
            RestoreBattleRewards(restoredRewardSystem, resolver, rewardOptions);
        }
        else
        {
            battleModalView.Show(currentBattleData);
            battleModalView.ClearStatus();
            RefreshActionButton();
        }

        if (hasPendingBattleDice)
            StartCoroutine(ApplyBattleDiceAfterAnimation(pendingBattleDiceValue));

        // Если бросок уже выпал до закрытия игры, после продолжения он доигрывается без повторного броска
        if (hasPendingEscapeRoll)
            StartCoroutine(ResolveEscapeAfterAnimation(pendingEscapeRollValue));

        BattleStateChanged?.Invoke();
    }
    /// <summary>
    /// Передает системе сохранений данные, которые можно будет найти по id
    /// </summary>
    public void RegisterSaveContent(GameSaveContentResolver resolver)
    {
        if (resolver == null || enemies == null)
            return;

        for (var i = 0; i < enemies.Count; i++)
            resolver.AddEnemy(enemies[i]);
    }
    /// <summary>
    /// Запускает боевой кубик, если он может помочь игроку в бою
    /// </summary>
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

        var diceValue = diceSystem.Roll();
        battleDiceUsed = true;
        hasPendingBattleDice = true;
        pendingBattleDiceValue = diceValue;
        StartCoroutine(ApplyBattleDiceAfterAnimation(diceValue));
        BattleStateChanged?.Invoke();
        return true;
    }
    /// <summary>
    /// Дожидается анимации кубика и добавляет выпавшее число к силе игрока в бою
    /// </summary>
    private IEnumerator ApplyBattleDiceAfterAnimation(int diceValue)
    {
        yield return DiceRollAnimationPlayer.PlayGlobalRoutine(diceValue, DiceRollAnimationContext.Battle);

        if (!IsBattleActive || currentEnemy == null)
            yield break;

        currentBattleDiceBonus = diceValue;
        hasPendingBattleDice = false;
        pendingBattleDiceValue = 0;
        currentBattleData = CreateBattleData(currentEnemy);
        battleModalView.Show(currentBattleData);
        RefreshActionButton();
        battleModalView.ShowTemporaryStatus($"Сила кубика: +{currentBattleDiceBonus}", TemporaryBattleStatusDuration);
        Debug.Log($"Battle dice used: +{currentBattleDiceBonus}. Player total is now {currentBattleData.PlayerTotalPower}.");
        BattleStateChanged?.Invoke();
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
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
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
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
    /// <summary>
    /// Перерисовывает открытое окно боя без смены текущего монстра
    /// </summary>
    public void RefreshCurrentBattleView(string status)
    {
        RefreshBattleModal(status);
        BattleStateChanged?.Invoke();
    }
    /// <summary>
    /// Обрабатывает нажатие главной кнопки боя и решает, что должно произойти дальше
    /// </summary>
    public void ResolveCurrentBattle()
    {
        if (currentBattleData == null)
            return;

        // Пока идет анимация побега, новый результат не считается
        if (escapeRollInProgress)
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

        // Итог боя решается по уже посчитанной силе игрока и монстра
        if (currentBattleData.PlayerTotalPower > currentBattleData.EnemyTotalPower)
            ResolveVictory();
        else
            RollEscape();
    }
    /// <summary>
    /// Подписывает систему боя на кнопку тестового боя и события наград
    /// </summary>
    private void OnEnable()
    {
        if (battleModalView != null)
            battleModalView.ResolveRequested += ResolveCurrentBattle;
    }
    /// <summary>
    /// Отписывает систему боя от тестовых кнопок и событий наград
    /// </summary>
    private void OnDisable()
    {
        if (battleModalView != null)
            battleModalView.ResolveRequested -= ResolveCurrentBattle;
    }
    /// <summary>
    /// Бросает кубик побега и решает, удалось ли игроку сбежать из боя
    /// </summary>
    private void RollEscape()
    {
        var escapeRoll = diceSystem.Roll();
        escapeRollInProgress = true;
        hasPendingEscapeRoll = true;
        pendingEscapeRollValue = escapeRoll;
        BattleStateChanged?.Invoke();
        StartCoroutine(ResolveEscapeAfterAnimation(escapeRoll));
    }
    /// <summary>
    /// Ждет анимацию кубика побега и затем применяет результат побега
    /// </summary>
    private IEnumerator ResolveEscapeAfterAnimation(int escapeRoll)
    {
        yield return DiceRollAnimationPlayer.PlayGlobalRoutine(escapeRoll, DiceRollAnimationContext.Escape);

        if (!IsBattleActive)
            yield break;

        escapeRollInProgress = false;
        hasPendingEscapeRoll = false;
        pendingEscapeRollValue = 0;
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
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    private void ResolveVictory()
    {
        var previousLevel = playerStats.Level;
        playerStats.SetLevel(playerStats.Level + 1);
        NotifyEffect(EffectType.Level, playerStats.Level - previousLevel, EffectNotificationStatus.Success);
        Debug.Log($"Battle won: {currentBattleData.PlayerName} defeated {currentBattleData.EnemyName}. Level is now {playerStats.Level}.");

        if (rewardSystem != null)
        {
            phase = BattlePhase.WaitingForReward;
            if (rewardSystem.ShowBattleRewards(HandleRewardAccepted))
            {
                battleModalView.Hide();
                BattleStateChanged?.Invoke();
                return;
            }

            phase = BattlePhase.WaitingForResolve;
        }

        CompleteBattle();
    }
    /// <summary>
    /// Применяет штраф за проигранный бой или неудачный побег
    /// </summary>
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
    /// <summary>
    /// Применяет конкретный эффект штрафа от монстра
    /// </summary>
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
    /// <summary>
    /// Отнимает здоровье у игрока по штрафу монстра
    /// </summary>
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
    /// <summary>
    /// Снижает уровень игрока по штрафу монстра
    /// </summary>
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
    /// <summary>
    /// Удаляет карты из руки игрока по штрафу монстра
    /// </summary>
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
    /// <summary>
    /// Выбирает, какие усиления получит монстр в этом бою
    /// </summary>
    private void SelectRandomModifiers(EnemyData enemy, List<EnemyModifier> selectedModifiers)
    {
        selectedModifiers.Clear();

        if (enemy == null || enemy.Modifiers == null || enemy.Modifiers.Count == 0)
            return;

        var validModifiers = new List<EnemyModifier>();
        for (var i = 0; i < enemy.Modifiers.Count; i++)
        {
            if (enemy.Modifiers[i] != null)
                validModifiers.Add(enemy.Modifiers[i]);
        }

        if (validModifiers.Count == 0)
            return;

        // Так строятся все варианты выпадения усилений: ничего, каждое отдельно и все сочетания
        var outcomeCount = 1 << validModifiers.Count;
        var selectedOutcome = UnityEngine.Random.Range(0, outcomeCount);

        for (var i = 0; i < validModifiers.Count; i++)
        {
            if ((selectedOutcome & (1 << i)) != 0)
                selectedModifiers.Add(validModifiers[i]);
        }

        Debug.Log($"{enemy.EnemyName} modifier outcome {selectedOutcome + 1}/{outcomeCount}: {(selectedModifiers.Count > 0 ? FormatModifierNames(selectedModifiers) : "none")}.");
    }
    /// <summary>
    /// Возвращает прибавку силы от одного модификатора монстра
    /// </summary>
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
    /// <summary>
    /// Очищает текущий бой и возвращает ход обратно полю
    /// </summary>
    private void CompleteBattle()
    {
        battleModalView.Hide();
        currentBattleData = null;
        currentEnemy = null;
        currentEnemyModifiers.Clear();
        phase = BattlePhase.None;
        currentBattleDiceBonus = 0;
        temporaryCardPowerBonus = 0;
        temporaryEscapeBonus = 0;
        battleDiceUsed = false;
        escapeRollInProgress = false;
        hasPendingBattleDice = false;
        pendingBattleDiceValue = 0;
        hasPendingEscapeRoll = false;
        pendingEscapeRollValue = 0;
        BattleStateChanged?.Invoke();

        var onCompleted = battleCompleted;
        battleCompleted = null;
        onCompleted?.Invoke();
    }
    /// <summary>
    /// Возвращает индексы модификаторов, которые сейчас выпали монстру
    /// </summary>
    private List<int> GetCurrentModifierIndexes()
    {
        var indexes = new List<int>();
        if (currentEnemy == null || currentEnemy.Modifiers == null)
            return indexes;

        for (var i = 0; i < currentEnemyModifiers.Count; i++)
        {
            var modifier = currentEnemyModifiers[i];
            for (var modifierIndex = 0; modifierIndex < currentEnemy.Modifiers.Count; modifierIndex++)
            {
                if (ReferenceEquals(currentEnemy.Modifiers[modifierIndex], modifier))
                {
                    indexes.Add(modifierIndex);
                    break;
                }
            }
        }

        return indexes;
    }
    /// <summary>
    /// Восстанавливает модификаторы монстра по индексам из сохранения
    /// </summary>
    private void RestoreModifierIndexes(EnemyData enemy, IReadOnlyList<int> modifierIndexes)
    {
        if (enemy == null || enemy.Modifiers == null || modifierIndexes == null)
            return;

        for (var i = 0; i < modifierIndexes.Count; i++)
        {
            var modifierIndex = modifierIndexes[i];
            if (modifierIndex >= 0 && modifierIndex < enemy.Modifiers.Count && enemy.Modifiers[modifierIndex] != null)
                currentEnemyModifiers.Add(enemy.Modifiers[modifierIndex]);
        }
    }
    /// <summary>
    /// Возвращает на экран те же награды после продолжения сохранения
    /// </summary>
    private void RestoreBattleRewards(RewardSystem restoredRewardSystem, GameSaveContentResolver resolver, IReadOnlyList<RewardSaveData> rewardOptions)
    {
        if (restoredRewardSystem == null || resolver == null || rewardOptions == null)
        {
            CompleteBattle();
            return;
        }

        var rewards = new List<RewardData>();
        for (var i = 0; i < rewardOptions.Count; i++)
        {
            var reward = resolver.GetReward(rewardOptions[i]);
            if (reward != null)
                rewards.Add(reward);
        }

        if (!restoredRewardSystem.RestoreBattleRewards(rewards, HandleRewardAccepted))
            CompleteBattle();
    }
    /// <summary>
    /// Закрывает бой после того, как игрок забрал награду
    /// </summary>
    private void HandleRewardAccepted()
    {
        Debug.Log("Battle reward accepted. Completing battle.");
        CompleteBattle();
    }
    /// <summary>
    /// Выбирает монстра для боя с учетом баланса по силе игрока
    /// </summary>
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

        var balancedEnemies = GetBalancedEnemies(validEnemies);
        var enemyPool = balancedEnemies.Count > 0 ? balancedEnemies : validEnemies;
        return enemyPool[UnityEngine.Random.Range(0, enemyPool.Count)];
    }
    /// <summary>
    /// Находит монстров, подходящих под текущую силу игрока
    /// </summary>
    private List<EnemyData> GetBalancedEnemies(IReadOnlyList<EnemyData> validEnemies)
    {
        var balancedEnemies = new List<EnemyData>();
        var playerPower = GetPlayerBalancePower();
        var minEnemyLevel = playerPower - enemyBalanceLowerOffset;
        var maxEnemyLevel = playerPower + enemyBalanceUpperOffset;

        // Баланс подбирает врага только по базовому уровню, без учета будущих усилений
        for (var i = 0; i < validEnemies.Count; i++)
        {
            var enemy = validEnemies[i];
            if (enemy.BaseLevel >= minEnemyLevel && enemy.BaseLevel <= maxEnemyLevel)
                balancedEnemies.Add(enemy);
        }

        return balancedEnemies;
    }
    /// <summary>
    /// Считает силу игрока для подбора монстра без боевого кубика и временных карт
    /// </summary>
    private int GetPlayerBalancePower()
    {
        var levelPower = playerStats != null ? playerStats.Level : 0;
        return levelPower + equipmentBonus + GetEquipmentEffectBonus(EffectType.Power);
    }
    /// <summary>
    /// Собирает данные, которые будут показаны в окне боя
    /// </summary>
    private BattleModalData CreateBattleData(EnemyData enemy)
    {
        // Строки силы нужны, чтобы игрок видел из чего сложилась итоговая сила
        var totalEquipmentBonus = equipmentBonus + GetEquipmentEffectBonus(EffectType.Power);
        var playerEntries = new List<BattlePowerEntry>
        {
            new BattlePowerEntry("Уровень", playerStats.Level),
            new BattlePowerEntry("Бонус экипировки", totalEquipmentBonus)
        };

        if (totalEquipmentBonus == 0)
            playerEntries.RemoveAt(playerEntries.Count - 1);

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

        for (var i = 0; i < currentEnemyModifiers.Count; i++)
        {
            var modifier = currentEnemyModifiers[i];
            var modifierName = string.IsNullOrEmpty(modifier.ModifierName)
                ? "Модификатор"
                : modifier.ModifierName;
            enemyEntries.Add(new BattlePowerEntry(modifierName, GetModifierPower(modifier)));
        }

        return new BattleModalData(
            PlayerName,
            playerSprite,
            playerEntries,
            SumEntries(playerEntries),
            enemy.EnemyName,
            enemy.EnemySprite,
            enemyEntries,
            SumEntries(enemyEntries));
    }
    /// <summary>
    /// Складывает все строки силы в одно итоговое число
    /// </summary>
    private static int SumEntries(IReadOnlyList<BattlePowerEntry> entries)
    {
        var total = 0;
        for (var i = 0; i < entries.Count; i++)
            total += entries[i].Value;

        return total;
    }
    /// <summary>
    /// Собирает имена модификаторов монстра в строку через запятую
    /// </summary>
    private static string FormatModifierNames(IReadOnlyList<EnemyModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
            return string.Empty;

        var names = new List<string>();
        for (var i = 0; i < modifiers.Count; i++)
        {
            var modifier = modifiers[i];
            names.Add(modifier == null || string.IsNullOrEmpty(modifier.ModifierName)
                ? "Modifier"
                : modifier.ModifierName);
        }

        return string.Join(", ", names);
    }
    /// <summary>
    /// Выводит в консоль информацию о монстре и его возможных усилениях
    /// </summary>
    private void LogBattleOpened(EnemyData enemy)
    {
        if (enemy == null)
            return;

        var modifierText = currentEnemyModifiers.Count > 0
            ? FormatModifierNames(currentEnemyModifiers)
            : "none";
        var possibleModifierText = enemy.Modifiers != null && enemy.Modifiers.Count > 0
            ? FormatModifierNames(enemy.Modifiers)
            : "none";

        Debug.Log($"Battle opened. Enemy: {enemy.EnemyName}, base level: {enemy.BaseLevel}, possible modifiers: {possibleModifierText}, selected modifiers: {modifierText}.");
    }
    /// <summary>
    /// Возвращает, насколько сила монстра больше силы игрока
    /// </summary>
    private int GetPowerDifference()
    {
        return currentBattleData == null ? 0 : currentBattleData.EnemyTotalPower - currentBattleData.PlayerTotalPower;
    }
    /// <summary>
    /// Считает бонус нужного типа от надетых предметов
    /// </summary>
    private int GetEquipmentEffectBonus(EffectType effectType)
    {
        return playerInventory != null ? playerInventory.GetTotalEffectValue(effectType) : 0;
    }
    /// <summary>
    /// Обновляет окно боя по текущему монстру, модификаторам и силе сторон
    /// </summary>
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
    /// <summary>
    /// Показывает всплывающую подсказку о штрафе, бонусе или другом эффекте боя
    /// </summary>
    private void NotifyEffect(EffectType effectType, int value, EffectNotificationStatus status)
    {
        if (eventNotificationSystem != null)
            eventNotificationSystem.ShowEffectNotification(effectType, value, status);
    }
    /// <summary>
    /// Возвращает текст главной кнопки боя для текущей фазы
    /// </summary>
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
    /// <summary>
    /// Включает или выключает главную кнопку боя и обновляет ее подпись
    /// </summary>
    private void RefreshActionButton()
    {
        if (battleModalView != null)
            battleModalView.SetActionButtonText(GetCurrentActionButtonText());
    }
}
