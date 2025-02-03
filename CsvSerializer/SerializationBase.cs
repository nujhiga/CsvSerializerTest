namespace CsvSerializer;
public abstract class CsvSerializationBase(SerializationOptions options)
{
    public SerializationOptions Options { get; private set; } = options;
    internal ObjectProperties Properties { get; private set; } = null!;
    protected void InitializeProperties<T>(char delimiter) where T : class
    {
        ArgumentNullException.ThrowIfNull(Options, nameof(Options));
        Properties = ObjectProperties.CreateProperties<T>(Options, delimiter);
    }
}
