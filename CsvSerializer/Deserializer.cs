using CsvFileHandler.IO;

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CsvSerializer;
public sealed class Deserializer : CsvSerializationBase
{
    public Deserializer(string filePath, char delimiter, Encoding encoding, SerializationOptions options) : base(options)
    {
        CsvReaderOptions readerOptions = new(encoding, delimiter);
        reader = new CsvReader(filePath, readerOptions);
    }
    public Deserializer(string filePath, CsvReaderOptions readerOptions, SerializationOptions options) : base(options)
    {
        reader = new CsvReader(filePath, readerOptions);
    }

    internal readonly CsvReader reader;
    internal ObjectFactory factory = null!;
    private bool headerMode;

    public IEnumerable<T> Deserialize<T>() where T : class, new()
    {
        Initialize<T>();
        bool headersModeFlag = Options.HeadersMode is not HeadersMode.None and not HeadersMode.FromType;

        foreach (var line in reader.ReadLines())
        {
            if (headersModeFlag)
            {
                headersModeFlag = false;
                if (headerMode)
                {
                    if (!factory.InitHeaders(line, reader.Delimiter))
                        yield break;
                }

                continue;
            }

            string[] ldata = line.Split(reader.Delimiter);
            yield return factory.CreateObject<T>(ldata, Options.Mode);
        }

        if (headerMode) factory.CleanHeaders();
    }
    public ConcurrentBag<T> ParallelDeserialize<T>() where T : class, new()
    {
        Initialize<T>(true);
        char lclDelimiter = reader.Delimiter;
        SerializationMode lclSerMode = Options.Mode;

        ConcurrentBag<T> objs = [];
        
        reader.ReadLines().AsParallel().ForAll(ln =>
        {
            string[] ldata = ln.Split(lclDelimiter);
            
            T obj = factory.CreateObject<T>(ldata, lclSerMode);
            if (obj is null) return;

            objs.Add(obj);
        });

        return objs;
    }

    public TCollection Deserialize<T, TCollection>()
        where T : class, new()
        where TCollection : ICollection<T>, new()
    {
        TCollection collection = new();
        var objs = Deserialize<T>();

        foreach (var obj in objs)
            collection.Add(obj);

        return collection;
    }
    internal void Initialize<T>(bool parallelDeserialize = false) where T : class
    {
        ValidateSerializationMode(parallelDeserialize);

        headerMode = Options.Mode is SerializationMode.Header;

        InitializeProperties<T>(reader.Delimiter);
        factory = new ObjectFactory(Properties, Options);
    }
    private void ValidateSerializationMode(bool parallelDeserialize)
    {
        if (Options.Mode is SerializationMode.Header)
        {
            if (Options.HeadersMode is HeadersMode.None or HeadersMode.OrdinalIgnore)
                throw new ArgumentException($"{nameof(Initialize)}: {nameof(HeadersMode)} can not be {nameof(HeadersMode.None)} or {nameof(HeadersMode.OrdinalIgnore)} when Options.Mode is {SerializationMode.Header}");
            if (parallelDeserialize && Options.HeadersMode is not HeadersMode.FromType)
                throw new ArgumentException($"{nameof(Initialize)}: {nameof(HeadersMode)} only can be {nameof(HeadersMode.FromType)} when {nameof(ParallelDeserialize)} method is used.");
        }

        if (Options.Mode is SerializationMode.Ordinal && Options.HeadersMode is HeadersMode.FromFile or HeadersMode.FromType)
            throw new ArgumentException($"{nameof(Initialize)}: {nameof(HeadersMode)} can not be {nameof(HeadersMode.FromFile)} or {nameof(HeadersMode.FromType)} when Options.Mode is {SerializationMode.Ordinal}");
    }
}

internal sealed class ObjectFactory
{
    public ObjectFactory(ObjectProperties properties, SerializationOptions options)
    {
        this.properties = properties;
        propertiesCount = properties.Count;

        if (options.HeadersMode is HeadersMode.FromType)
            InitHeaders();
    }

    private readonly ObjectProperties properties;
    private readonly int propertiesCount;
    private string[] headers = null!;
    private int headersLen = 0;

    public T CreateObject<T>(string[] ldata, SerializationMode mode) where T : class, new()
    {
        return mode switch
        {
            SerializationMode.Ordinal => GetObjectOrdinal<T>(ldata),
            SerializationMode.Header => GetObjectHeader<T>(ldata),
            _ => null!
        };
    }

    private T GetObjectOrdinal<T>(string[] ldata) where T : class, new()
    {
        int ldataLength = ldata.Length;
        if (ldataLength != propertiesCount) return null!;

        T obj = new();

        foreach (var prop in properties)
        {
            if (prop.Ignore) continue;

            var pi = prop.Pindex;
            var ptype = prop.Ptype;
            var pinfo = prop.Pinfo;

            object value = GetDataValue(ldata[pi], ptype);
            pinfo.SetValue(obj, value);
        }

        return obj;
    }
    private T GetObjectHeader<T>(string[] ldata) where T : class, new()
    {
        T obj = new();
        for (int i = 0; i < headersLen; i++)
        {
            string hd = headers[i];
            if (string.IsNullOrWhiteSpace(hd)) continue;
            if (hd == ldata[i]) return null!;

            var prop = properties[hd];
            if (prop.Ignore) continue;

            var ptype = prop.Ptype;
            var pinfo = prop.Pinfo;
            
            object value = GetDataValue(ldata[i], ptype);
            pinfo.SetValue(obj, value);
        }

        return obj;
    }

    //private T GetObjectOrdinal<T>(string[] ldata, out object? key) where T : class, new()
    //{
    //    key = null;
    //    int ldataLength = ldata.Length;
    //    if (ldataLength != propertiesCount) return null!;

    //    T obj = new();

    //    foreach (var prop in properties)
    //    {
    //        if (prop.Ignore) continue;

    //        var pi = prop.Pindex;
    //        var ptype = prop.Ptype;
    //        var pinfo = prop.Pinfo;

    //        object value = GetDataValue(ldata[pi], ptype);
    //        pinfo.SetValue(obj, value);

    //        if (prop.IsKey) key = value;
    //    }

    //    return obj;
    //}
    //private T GetObjectHeader<T>(string[] ldata, out object? key) where T : class, new()
    //{
    //    T obj = new();
    //    key = null;
    //    for (int i = 0; i < headersLen; i++)
    //    {
    //        string hd = headers[i];
    //        if (string.IsNullOrWhiteSpace(hd)) continue;

    //        var prop = properties[hd];
    //        if (prop.Ignore) continue;

    //        var ptype = prop.Ptype;
    //        var pinfo = prop.Pinfo;

    //        { }
    //        //object value = GetDataValue(data, ptype, prop.Pindex, reader.Lindex, ldata);
    //        object value = GetDataValue(ldata[i], ptype);
    //        pinfo.SetValue(obj, value);
    //        if (prop.IsKey) key = value;
    //    }

    //    return obj;
    //}
    //private static object GetDataValue(string data, Type tProp, int i = 0, int ii = -1, string[] ldata = null)
    private static object GetDataValue(string data, Type tProp)
    {
        if (string.IsNullOrWhiteSpace(data)) return null!;

        if (tProp.IsEnum)
            return GetDataEnum(data, tProp);

        object value = tProp switch
        {
            { } when tProp == typeof(decimal) => Convert.ToDecimal(data, CultureInfo.InvariantCulture),
            { } when tProp == typeof(double) => Convert.ToDouble(data, CultureInfo.InvariantCulture),
            { } when tProp == typeof(float) => Convert.ToSingle(data, CultureInfo.InvariantCulture),
            _ => Convert.ChangeType(data, tProp!)
        };
        
        return value;
    }
    private static object GetDataEnum(string data, Type tProp)
    {
        // Console.WriteLine(data);
        bool parsed = Enum.TryParse(tProp, data, false, out var enumValue);
        return parsed ? enumValue! : Enum.Parse(tProp, "0");
        //return parsed ? enumValue! : Convert.ChangeType(0, tProp);
    }
    public bool InitHeaders(string line, char delimiter)
    {
        headers = line.Split(delimiter);
        if (headers.Length <= 0) return false;
        headersLen = headers.Length;
        return true;
    }
    private void InitHeaders()
    {
        headers = properties.AsHeaders();
        headersLen = headers.Length;
    }
    public void CleanHeaders()
    {
        if (headers is not null)
            Array.Clear(headers, 0, headers.Length);
    }
    public static object GetObjectPropertyValue<T>(T obj, int propIndex)
    {
        PropertyInfo pinfo = typeof(T).GetProperties()[propIndex];
        if (pinfo is null) return null!;

        return pinfo.GetValue(obj);
    }
}
