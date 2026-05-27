using System;
using System.Collections.Generic;
/// <summary>
/// Все данные сохраненной партии, которые записываются в JSON-файл
/// </summary>

[Serializable]
public sealed class GameSaveData
{
    public int version = 1;
    public int currentHp;
    public int level;
    public int currentTileIndex;
    public List<string> cardIds = new List<string>();
    public List<string> itemIds = new List<string>();
    public TurnSaveData turn;
    public BattleSaveData battle;
    public List<RewardSaveData> battleRewardOptions = new List<RewardSaveData>();
    public RewardSaveData singleReward;
}
/// <summary>
/// Данные сохраненного хода, если игрок закрыл игру во время движения или обработки клетки
/// </summary>
[Serializable]
public sealed class TurnSaveData
{
    public int state;
    public bool hasPendingBoardMove;
    public int pendingBoardMoveSteps;
    public bool pendingBoardMoveShowsDice;
    public int pendingBoardMoveStartTileIndex;
}
/// <summary>
/// Данные сохраненного боя, включая монстра, модификаторы, фазу боя и уже выпавшие броски
/// </summary>
[Serializable]
public sealed class BattleSaveData
{
    public bool active;
    public string enemyId;
    public List<int> modifierIndexes = new List<int>();
    public int phase;
    public int currentBattleDiceBonus;
    public int temporaryCardPowerBonus;
    public int temporaryEscapeBonus;
    public bool battleDiceUsed;
    public bool hasPendingBattleDice;
    public int pendingBattleDiceValue;
    public bool hasPendingEscapeRoll;
    public int pendingEscapeRollValue;
}
/// <summary>
/// Данные сохраненной награды, чтобы после продолжения показать тот же приз
/// </summary>
[Serializable]
public sealed class RewardSaveData
{
    public int rewardType;
    public string contentId;
}
