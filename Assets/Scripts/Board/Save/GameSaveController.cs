using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Собирает данные партии для сохранения и восстанавливает их при нажатии Продолжить
/// </summary>

public sealed class GameSaveController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private SingleRewardSystem singleRewardSystem;
    [SerializeField] private TileEffectSystem tileEffectSystem;

    private readonly GameSaveContentResolver contentResolver = new GameSaveContentResolver();
    private bool isRestoring;
    private bool initialized;
    private bool savingDisabled;

    public static GameSaveController Instance { get; private set; }
    /// <summary>
    /// Сохраняет текущую партию, если сохранение сейчас разрешено
    /// </summary>
    public void SaveNow()
    {
        if (savingDisabled || isRestoring || !initialized)
            return;

        GameSaveService.Save(CreateSaveData());
    }
    /// <summary>
    /// Сохраняет партию перед выходом, даже если сцена еще не полностью готова
    /// </summary>
    public void SaveNowEvenIfInitializing()
    {
        if (savingDisabled || isRestoring)
            return;

        if (!initialized && GameLaunchIntent.Mode == GameLaunchMode.Continue)
            return;

        GameSaveService.Save(CreateSaveData());
    }
    /// <summary>
    /// Удаляет сохранение завершенной партии и запрещает создать его заново
    /// </summary>
    public void DeleteSaveAndDisableSaving()
    {
        savingDisabled = true;
        GameSaveService.DeleteSave();
    }
    /// <summary>
    /// Находит системы сцены, которые нужно сохранить или восстановить
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResolveReferences();
        BuildContentResolver();
    }
    /// <summary>
    /// Запускает начальную настройку после загрузки сцены
    /// </summary>
    private void Start()
    {
        var launchMode = GameLaunchIntent.Consume();
        if (launchMode == GameLaunchMode.Continue && GameSaveService.TryLoad(out var saveData))
            Restore(saveData);

        initialized = true;
    }
    /// <summary>
    /// Подписывает автосохранение на изменения партии
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
    }
    /// <summary>
    /// Отписывает автосохранение от изменений партии
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();

        if (Instance == this)
            Instance = null;
    }
    /// <summary>
    /// Реагирует на паузу приложения, например при сворачивании
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveNowEvenIfInitializing();
    }
    /// <summary>
    /// Сохраняет или обновляет состояние при потере и возврате фокуса приложения
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveNowEvenIfInitializing();
    }
    /// <summary>
    /// Сохраняет текущую партию при закрытии приложения
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveNowEvenIfInitializing();
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    private void ResolveReferences()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        if (cardSystem == null)
            cardSystem = FindAnyObjectByType<CardSystem>();
        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();
        if (playerMover == null)
            playerMover = FindAnyObjectByType<PlayerMover>();
        if (turnSystem == null)
            turnSystem = FindAnyObjectByType<TurnSystem>();
        if (battleSystem == null)
            battleSystem = FindAnyObjectByType<BattleSystem>();
        if (rewardSystem == null)
            rewardSystem = FindAnyObjectByType<RewardSystem>();
        if (singleRewardSystem == null)
            singleRewardSystem = FindAnyObjectByType<SingleRewardSystem>();
        if (tileEffectSystem == null)
            tileEffectSystem = FindAnyObjectByType<TileEffectSystem>();
    }
    /// <summary>
    /// Собирает справочник карт, предметов и врагов для загрузки сохранения
    /// </summary>
    private void BuildContentResolver()
    {
        rewardSystem?.RegisterSaveContent(contentResolver);
        tileEffectSystem?.RegisterSaveContent(contentResolver);
        battleSystem?.RegisterSaveContent(contentResolver);

        if (cardSystem != null)
        {
            var hand = cardSystem.Hand;
            for (var i = 0; i < hand.Count; i++)
                contentResolver.AddCard(hand[i]);
        }

        if (playerInventory != null)
        {
            var items = playerInventory.GetEquippedItems();
            for (var i = 0; i < items.Count; i++)
                contentResolver.AddItem(items[i]);
        }
    }
    /// <summary>
    /// Собирает все данные партии в один объект для записи в файл
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        // В файл пишутся простые данные и id, а настоящие Unity-ссылки находятся заново при загрузке
        var saveData = new GameSaveData
        {
            currentHp = playerStats != null ? playerStats.CurrentHp : 0,
            level = playerStats != null ? playerStats.Level : 1,
            currentTileIndex = boardManager != null ? boardManager.CurrentIndex : 0,
            turn = turnSystem != null ? turnSystem.CaptureSaveData() : null,
            battle = battleSystem != null ? battleSystem.CaptureSaveData() : null,
            singleReward = singleRewardSystem != null ? CreateRewardSaveData(singleRewardSystem.CurrentReward) : null
        };

        AddCardIds(saveData.cardIds);
        AddItemIds(saveData.itemIds);
        AddRewardOptions(saveData.battleRewardOptions);
        return saveData;
    }
    /// <summary>
    /// Восстанавливает игрока, поле, бой и награды из сохранения
    /// </summary>
    private void Restore(GameSaveData saveData)
    {
        if (saveData == null)
            return;

        // На время загрузки autosave выключается, чтобы не записать половину восстановленного состояния
        isRestoring = true;
        try
        {
            // Сначала возвращаются здоровье, уровень, позиция, карты и предметы игрока
            playerStats?.RestoreState(saveData.currentHp, saveData.level);
            boardManager?.SetCurrentIndex(saveData.currentTileIndex);
            playerMover?.SnapToCurrentTile();
            RestoreCards(saveData.cardIds);
            RestoreItems(saveData.itemIds);

            var hasActiveBattle = saveData.battle != null && saveData.battle.active;
            var hasActiveSingleReward = saveData.singleReward != null && !string.IsNullOrEmpty(saveData.singleReward.contentId);
            turnSystem?.RestoreFromSave(saveData.turn, hasActiveBattle || hasActiveSingleReward);

            // Окна боя и наград восстанавливаются последними, потому что они могут завершить обработку клетки
            if (hasActiveBattle)
                battleSystem?.RestoreFromSave(saveData.battle, contentResolver, rewardSystem, saveData.battleRewardOptions, turnSystem != null ? turnSystem.CompleteRestoredTileResolution : null);
            else if (hasActiveSingleReward && singleRewardSystem != null)
            {
                var restoredReward = contentResolver.GetReward(saveData.singleReward);
                if (!singleRewardSystem.RestoreReward(restoredReward, turnSystem != null ? turnSystem.CompleteRestoredTileResolution : null))
                    turnSystem?.CompleteRestoredTileResolution();
            }
        }
        finally
        {
            isRestoring = false;
        }
    }
    /// <summary>
    /// Восстанавливает руку игрока по id карт из сохранения
    /// </summary>
    private void RestoreCards(List<string> cardIds)
    {
        if (cardSystem == null)
            return;

        var cards = new List<CardData>();
        if (cardIds != null)
        {
            for (var i = 0; i < cardIds.Count; i++)
            {
                var card = contentResolver.GetCard(cardIds[i]);
                if (card != null)
                    cards.Add(card);
            }
        }

        cardSystem.SetHand(cards);
    }
    /// <summary>
    /// Восстанавливает экипировку игрока по id предметов из сохранения
    /// </summary>
    private void RestoreItems(List<string> itemIds)
    {
        if (playerInventory == null)
            return;

        var items = new List<ItemData>();
        if (itemIds != null)
        {
            for (var i = 0; i < itemIds.Count; i++)
            {
                var item = contentResolver.GetItem(itemIds[i]);
                if (item != null)
                    items.Add(item);
            }
        }

        playerInventory.SetEquipment(items);
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    private void AddCardIds(List<string> cardIds)
    {
        if (cardSystem == null || cardIds == null)
            return;

        var hand = cardSystem.Hand;
        for (var i = 0; i < hand.Count; i++)
        {
            var card = hand[i];
            if (card != null && !string.IsNullOrEmpty(card.CardId))
                cardIds.Add(card.CardId);
        }
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    private void AddItemIds(List<string> itemIds)
    {
        if (playerInventory == null || itemIds == null)
            return;

        var items = playerInventory.GetEquippedItems();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item != null && !string.IsNullOrEmpty(item.ItemId))
                itemIds.Add(item.ItemId);
        }
    }
    /// <summary>
    /// Добавляет новый элемент в игровое состояние
    /// </summary>
    private void AddRewardOptions(List<RewardSaveData> rewards)
    {
        if (rewardSystem == null || rewards == null)
            return;

        var currentRewards = rewardSystem.CurrentRewards;
        for (var i = 0; i < currentRewards.Count; i++)
        {
            var saveData = CreateRewardSaveData(currentRewards[i]);
            if (saveData != null)
                rewards.Add(saveData);
        }
    }
    /// <summary>
    /// Создает данные сохранения для карты или предмета-награды
    /// </summary>
    private static RewardSaveData CreateRewardSaveData(RewardData reward)
    {
        if (reward == null)
            return null;

        var contentId = reward.RewardType == RewardType.Item
            ? reward.ItemData != null ? reward.ItemData.ItemId : string.Empty
            : reward.CardData != null ? reward.CardData.CardId : string.Empty;

        return string.IsNullOrEmpty(contentId)
            ? null
            : new RewardSaveData { rewardType = (int)reward.RewardType, contentId = contentId };
    }
    /// <summary>
    /// Подписывает сохранение на важные изменения партии
    /// </summary>
    private void Subscribe()
    {
        if (playerStats != null)
        {
            playerStats.OnHpChanged += HandleStatsChanged;
            playerStats.OnLevelChanged += HandleLevelChanged;
        }

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged += HandleEquipmentChanged;
        if (cardSystem != null)
            cardSystem.OnHandChanged += HandleHandChanged;
        if (turnSystem != null)
        {
            turnSystem.StateChanged += HandleTurnStateChanged;
            turnSystem.DiceRolled += HandleDiceRolled;
            turnSystem.TurnEnded += HandleTurnEnded;
        }
        if (battleSystem != null)
            battleSystem.BattleStateChanged += HandleBattleStateChanged;
        if (rewardSystem != null)
            rewardSystem.RewardStateChanged += HandleRewardStateChanged;
        if (singleRewardSystem != null)
            singleRewardSystem.RewardStateChanged += HandleRewardStateChanged;
    }
    /// <summary>
    /// Отписывает сохранение от событий игровой сцены
    /// </summary>
    private void Unsubscribe()
    {
        if (playerStats != null)
        {
            playerStats.OnHpChanged -= HandleStatsChanged;
            playerStats.OnLevelChanged -= HandleLevelChanged;
        }

        if (playerInventory != null)
            playerInventory.OnEquipmentChanged -= HandleEquipmentChanged;
        if (cardSystem != null)
            cardSystem.OnHandChanged -= HandleHandChanged;
        if (turnSystem != null)
        {
            turnSystem.StateChanged -= HandleTurnStateChanged;
            turnSystem.DiceRolled -= HandleDiceRolled;
            turnSystem.TurnEnded -= HandleTurnEnded;
        }
        if (battleSystem != null)
            battleSystem.BattleStateChanged -= HandleBattleStateChanged;
        if (rewardSystem != null)
            rewardSystem.RewardStateChanged -= HandleRewardStateChanged;
        if (singleRewardSystem != null)
            singleRewardSystem.RewardStateChanged -= HandleRewardStateChanged;
    }
    /// <summary>
    /// Сохраняет партию после изменения здоровья игрока
    /// </summary>
    private void HandleStatsChanged(int currentHp, int maxHp) => SaveNow();
    /// <summary>
    /// Сохраняет партию после изменения уровня игрока
    /// </summary>
    private void HandleLevelChanged(int level) => SaveNow();
    /// <summary>
    /// Сохраняет партию после изменения экипировки
    /// </summary>
    private void HandleEquipmentChanged(IReadOnlyList<ItemData> items) => SaveNow();
    /// <summary>
    /// Сохраняет партию после изменения руки карт
    /// </summary>
    private void HandleHandChanged(IReadOnlyList<CardData> cards) => SaveNow();
    /// <summary>
    /// Сохраняет партию после смены состояния хода
    /// </summary>
    private void HandleTurnStateChanged(TurnState state) => SaveNow();
    /// <summary>
    /// Сохраняет партию сразу после выпадения кубика
    /// </summary>
    private void HandleDiceRolled(int value) => SaveNow();
    /// <summary>
    /// Сохраняет партию после завершения хода
    /// </summary>
    private void HandleTurnEnded() => SaveNow();
    /// <summary>
    /// Сохраняет партию после изменения состояния боя
    /// </summary>
    private void HandleBattleStateChanged() => SaveNow();
    /// <summary>
    /// Сохраняет партию после изменения состояния награды
    /// </summary>
    private void HandleRewardStateChanged() => SaveNow();
}
