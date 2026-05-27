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
    /// <param name="playerName">Имя игрока для окна боя</param>
    /// <param name="playerSprite">Картинка игрока для окна боя</param>
    /// <param name="playerPowerEntries">Строки расчета силы игрока</param>
    /// <param name="playerTotalPower">Итоговая сила игрока</param>
    /// <param name="enemyName">Имя монстра для окна боя</param>
    /// <param name="enemySprite">Картинка монстра для окна боя</param>
    /// <param name="enemyPowerEntries">Строки расчета силы монстра</param>
    /// <param name="enemyTotalPower">Итоговая сила монстра</param>
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

    /// <summary>
    /// Имя игрока, которое показывается в окне боя
    /// </summary>
    public string PlayerName { get; }
    /// <summary>
    /// Картинка игрока для окна боя
    /// </summary>
    public Sprite PlayerSprite { get; }
    /// <summary>
    /// Строки, из которых складывается сила игрока
    /// </summary>
    public IReadOnlyList<BattlePowerEntry> PlayerPowerEntries { get; }
    /// <summary>
    /// Итоговая сила игрока в текущем бою
    /// </summary>
    public int PlayerTotalPower { get; }
    /// <summary>
    /// Имя монстра, которое показывается в окне боя
    /// </summary>
    public string EnemyName { get; }
    /// <summary>
    /// Картинка монстра для окна боя
    /// </summary>
    public Sprite EnemySprite { get; }
    /// <summary>
    /// Строки, из которых складывается сила монстра
    /// </summary>
    public IReadOnlyList<BattlePowerEntry> EnemyPowerEntries { get; }
    /// <summary>
    /// Итоговая сила монстра в текущем бою
    /// </summary>
    public int EnemyTotalPower { get; }
}
