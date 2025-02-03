using CsvFileHandler.IO;

namespace CsvSerializer;

public static class CsvCollectionFactory
{
    public static CsvCollection<T> Create<T>(Deserializer deserializer, int maxCapacity = 0) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(deserializer);

        CsvCollection<T> csvColl = maxCapacity > 0 ? new(maxCapacity) : new();

        foreach (var dObj in deserializer.Deserialize<T>())
            csvColl.TryAdd(dObj);

        if (maxCapacity == 0)
            csvColl.EnsureCapacity(csvColl.Count);

        return csvColl;
    }

    public static CsvCollection<T> CreateParallel<T>(Deserializer deserializer, int maxCapacity = 0) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(deserializer);

        deserializer.Initialize<T>(true);

        CsvReader lclReader = deserializer.reader;
        ObjectFactory lclFactory = deserializer.factory;
        char lclDelimiter = lclReader.Delimiter;
        SerializationMode lclSerMode = deserializer.Options.Mode;

        CsvCollection<T> csvColl = maxCapacity > 0 ? new(maxCapacity) : new();

        lclReader.ReadLines().AsParallel().ForAll(ln =>
        {
            string[] ldata = ln.Split(lclDelimiter);

            T obj = lclFactory.CreateObject<T>(ldata, lclSerMode);
            if (obj is null) return;

            csvColl.TryAdd(obj);
        });

        if (maxCapacity == 0)
            csvColl.EnsureCapacity(csvColl.Count);

        return csvColl;
    }
}
