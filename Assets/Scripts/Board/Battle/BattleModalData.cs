using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Хранит данные боя, которые затем подставляются в модальное окно: имена, картинки и строки силы
/// </summary>

public sealed class BattleModalData
{
    /// <summary>
    /// Собирает все данные, которые окно боя должно показать игроку
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
