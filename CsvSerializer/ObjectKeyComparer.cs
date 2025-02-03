namespace CsvSerializer;
internal class ObjectKeyComparer : IEqualityComparer<object>
{
    public new bool Equals(object? x, object? y) => x?.Equals(y) ?? false;
    public int GetHashCode(object obj) => obj switch
    {
        int i => i,
        double d => d.GetHashCode(),
        decimal dd => dd.GetHashCode(),
        float f => f.GetHashCode(),
        DateTime dt => dt.GetHashCode(),
        string s => s.GetHashCode(),
        _ => obj.GetHashCode()
    };
}
