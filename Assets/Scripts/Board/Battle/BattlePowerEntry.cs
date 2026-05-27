/// <summary>
/// Одна строка в расчете силы, например уровень игрока, бонус предмета или модификатор монстра
/// </summary>
public readonly struct BattlePowerEntry
{
    /// <summary>
    /// Создает одну строку расчета силы с подписью и числом
    /// </summary>
    /// <param name="label">Подпись, которую увидит игрок</param>
    /// <param name="value">Число, которое строка добавляет к общей силе</param>
    public BattlePowerEntry(string label, int value)
    {
        Label = label;
        Value = value;
    }

    /// <summary>
    /// Подпись строки силы, например уровень или бонус предмета
    /// </summary>
    public string Label { get; }
    /// <summary>
    /// Число, которое эта строка добавляет к общей силе
    /// </summary>
    public int Value { get; }
}
