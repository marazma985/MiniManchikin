using System.Collections.Generic;
using UnityEngine;

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

    public static GameSaveController Instance { get; private set; }

    public void SaveNow()
    {
        if (isRestoring || !initialized)
            return;

        GameSaveService.Save(CreateSaveData());
    }

    public void SaveNowEvenIfInitializing()
    {
        if (isRestoring)
            return;

        if (!initialized && GameLaunchIntent.Mode == GameLaunchMode.Continue)
            return;

        GameSaveService.Save(CreateSaveData());
    }

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

    private void Start()
    {
        var launchMode = GameLaunchIntent.Consume();
        if (launchMode == GameLaunchMode.Continue && GameSaveService.TryLoad(out var saveData))
            Restore(saveData);

        initialized = true;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveNowEvenIfInitializing();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveNowEvenIfInitializing();
    }

    private void OnApplicationQuit()
    {
        SaveNowEvenIfInitializing();
    }

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

    private GameSaveData CreateSaveData()
    {
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

    private void Restore(GameSaveData saveData)
    {
        if (saveData == null)
            return;

        isRestoring = true;
        try
        {
            playerStats?.RestoreState(saveData.currentHp, saveData.level);
            boardManager?.SetCurrentIndex(saveData.currentTileIndex);
            playerMover?.SnapToCurrentTile();
            RestoreCards(saveData.cardIds);
            RestoreItems(saveData.itemIds);

            var hasActiveBattle = saveData.battle != null && saveData.battle.active;
            var hasActiveSingleReward = saveData.singleReward != null && !string.IsNullOrEmpty(saveData.singleReward.contentId);
            turnSystem?.RestoreFromSave(saveData.turn, hasActiveBattle || hasActiveSingleReward);

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

    private void HandleStatsChanged(int currentHp, int maxHp) => SaveNow();
    private void HandleLevelChanged(int level) => SaveNow();
    private void HandleEquipmentChanged(IReadOnlyList<ItemData> items) => SaveNow();
    private void HandleHandChanged(IReadOnlyList<CardData> cards) => SaveNow();
    private void HandleTurnStateChanged(TurnState state) => SaveNow();
    private void HandleDiceRolled(int value) => SaveNow();
    private void HandleTurnEnded() => SaveNow();
    private void HandleBattleStateChanged() => SaveNow();
    private void HandleRewardStateChanged() => SaveNow();
}
