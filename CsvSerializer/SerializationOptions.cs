using CsvSerializer.Filtering;

using System.Reflection;

namespace CsvSerializer;
public sealed class SerializationOptions(SerializationMode mode, HeadersMode hmode)
{
    public SerializationMode Mode { get; set; } = mode;
    public HeadersMode HeadersMode { get; set; } = hmode;
    public BindingFlags Flags { get; set; } = BindingFlags.Instance | BindingFlags.Public;

    public SerializationFilter Filter { get; private set; } = SerializationFilter.None;
    internal string[] FilterNames { get; private set; } = null!;
    internal Type[] FilterTypes { get; private set; }  = null!;

    internal int FilterLength { get; private set; }
    public void SetFilterNames(FilterMode mode, params string[] args)
    {
        if (args.Any(a => a is null))
            throw new ArgumentNullException(nameof(SetFilterNames), 
                $"An element of {nameof(args)} is null");

        Filter = new(mode, FilterScope.Name);
        FilterNames = args;
        FilterLength = args.Length;
    }
    public void SetFilterTypes(FilterMode mode, params Type[] args)
    {
        if (args.Any(a => a is null))
            throw new ArgumentNullException(nameof(SetFilterTypes),
                $"An element of {nameof(args)} is null");

        Filter = new(mode, FilterScope.Type);
        FilterTypes = args;
        FilterLength = args.Length;
    }
    public void CleanFilters()
    {
        Filter = SerializationFilter.None;
        FilterLength = 0;

        if (FilterNames is not null)
        {
            Array.Clear(FilterNames);
            FilterNames = [];
        }

        if (FilterTypes is not null)
        {
            Array.Clear(FilterTypes);
            FilterTypes = [];
        }
    }
}