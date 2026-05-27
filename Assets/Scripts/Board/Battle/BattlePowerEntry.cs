/// <summary>
/// Одна строка в расчете силы, например уровень игрока, бонус предмета или модификатор монстра
/// </summary>
public readonly struct BattlePowerEntry
{
    /// <summary>
    /// Создает одну строку расчета силы с подписью и числом
    /// </summary>
    public BattlePowerEntry(string label, int value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public int Value { get; }
}
