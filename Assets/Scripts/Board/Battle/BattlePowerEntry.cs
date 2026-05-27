/// <summary>
/// Хранит компактные данные battle power entry без собственного жизненного цикла Unity
/// </summary>
public readonly struct BattlePowerEntry
{
    /// <summary>
    /// Создает экземпляр BattlePowerEntry и заполняет его начальными данными
    /// </summary>
    public BattlePowerEntry(string label, int value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public int Value { get; }
}
