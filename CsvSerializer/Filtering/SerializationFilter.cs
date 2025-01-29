namespace CsvSerializer.Filtering;
public readonly struct SerializationFilter
{
    internal SerializationFilter(FilterMode mode = FilterMode.None, FilterScope scope = FilterScope.None)
    {
        Scope = scope;
        Mode = mode;
    }

    public readonly bool IsNone => Mode == FilterMode.None;

    public readonly FilterScope Scope;
    public readonly FilterMode Mode;

    public static SerializationFilter IgnoreNames => new(FilterMode.Ignore, FilterScope.Name);
    public static SerializationFilter IgnoreTypes => new(FilterMode.Ignore, FilterScope.Type);
    public static SerializationFilter ExclusiveNames => new(FilterMode.Exclusive, FilterScope.Name);
    public static SerializationFilter ExclusiveTypes => new(FilterMode.Exclusive, FilterScope.Type);
    public static SerializationFilter None => new();
}
