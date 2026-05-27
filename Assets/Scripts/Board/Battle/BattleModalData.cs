using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Отвечает за часть системы боя, связанную с BattleModalData
/// </summary>

public sealed class BattleModalData
{
    /// <summary>
    /// Создает экземпляр BattleModalData и заполняет его начальными данными
    /// </summary>
    public BattleModalData(
        string playerName,
        Sprite playerSprite,
        IReadOnlyList<BattlePowerEntry> playerPowerEntries,
        int playerTotalPower,
        string enemyName,
        Sprite enemySprite,
        IReadOnlyList<BattlePowerEntry> enemyPowerEntries,
        int enemyTotalPower)
    {
        PlayerName = playerName;
        PlayerSprite = playerSprite;
        PlayerPowerEntries = playerPowerEntries;
        PlayerTotalPower = playerTotalPower;
        EnemyName = enemyName;
        EnemySprite = enemySprite;
        EnemyPowerEntries = enemyPowerEntries;
        EnemyTotalPower = enemyTotalPower;
    }

    public string PlayerName { get; }
    public Sprite PlayerSprite { get; }
    public IReadOnlyList<BattlePowerEntry> PlayerPowerEntries { get; }
    public int PlayerTotalPower { get; }
    public string EnemyName { get; }
    public Sprite EnemySprite { get; }
    public IReadOnlyList<BattlePowerEntry> EnemyPowerEntries { get; }
    public int EnemyTotalPower { get; }
}
