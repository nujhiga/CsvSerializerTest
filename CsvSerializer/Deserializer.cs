using CsvFileHandler.IO;

using System.Collections.Immutable;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
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

    private bool headerMode;

    private readonly CsvReader reader;
    private ObjectFactory factory = null!;

    private void Initialize<T>(bool validateSerializationMode = true) where T : class
    {
        if (validateSerializationMode)
            ValidateSerializationMode();

        headerMode = Options.Mode is SerializationMode.Header;

        InitializeProperties<T>(reader.Delimiter);
        factory = new ObjectFactory(Properties, Options);
    }
    private void ValidateSerializationMode()
    {
        if (Options.Mode is SerializationMode.Header && Options.HeadersMode is HeadersMode.None or HeadersMode.OrdinalIgnore)
            throw new ArgumentException($"{nameof(Initialize)}: {nameof(HeadersMode)} can not be {nameof(HeadersMode.None)} or {nameof(HeadersMode.OrdinalIgnore)} when Options.Mode is {SerializationMode.Header}");
        if (Options.Mode is SerializationMode.Ordinal && Options.HeadersMode is HeadersMode.FromFile or HeadersMode.FromType)
            throw new ArgumentException($"{nameof(Initialize)}: {nameof(HeadersMode)} can not be {nameof(HeadersMode.FromFile)} or {nameof(HeadersMode.FromType)} when Options.Mode is {SerializationMode.Ordinal}");
    }

    public IEnumerable<T> Deserialize<T>() where T : class, new()
    {
        Initialize<T>();
        bool headersModeFlag = Options.HeadersMode is not HeadersMode.None;

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

    private class ObjectFactory
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

        public T CreateObject<T>(string[] ldata, SerializationMode mode) where T : class, new()
        {
            return mode switch
            {
                SerializationMode.Ordinal => GetObjectOrdinal<T>(ldata),
                SerializationMode.Header => GetObjectHeader<T>(ldata),
                _ => null!
            };
        }
        public T CreateObject<T>(ImmutableArray<string> ldata, SerializationMode mode) where T : class, new()
        {
            return mode switch
            {
                SerializationMode.Ordinal => GetObjectOrdinal<T>(ldata),
                //SerializationMode.Header => GetObjectHeader<T>(ldata),
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
        private T GetObjectOrdinal<T>(ImmutableArray<string> ldata) where T : class, new()
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

            for (int i = 0; i < headers.Length; i++)
            {
                string hd = headers[i];
                if (string.IsNullOrWhiteSpace(hd)) continue;

                var prop = properties[hd];
                if (prop.Ignore) continue;

                var ptype = prop.Ptype;
                var pinfo = prop.Pinfo;

                { }
                //object value = GetDataValue(data, ptype, prop.Pindex, reader.Lindex, ldata);
                object value = GetDataValue(ldata[i], ptype);
                pinfo.SetValue(obj, value);
            }

            return obj;
        }
        //private static object GetDataValue(string data, Type tProp, int i = 0, int ii = -1, string[] ldata = null)
        private static object GetDataValue(string data, Type tProp)
        {
            if (string.IsNullOrWhiteSpace(data)) return null!;

            if (tProp.IsEnum)
                return GetDataEnum(data, tProp);
                //return GetDataEnum(data, tProp, i, ii, ldata);

            object value = tProp switch
            {
                { } when tProp == typeof(decimal) => Convert.ToDecimal(data, CultureInfo.InvariantCulture),
                { } when tProp == typeof(double) => Convert.ToDouble(data, CultureInfo.InvariantCulture),
                _ => Convert.ChangeType(data, tProp)
            };

            return value;
        }
        //private static object GetDataEnum(string data, Type tProp, int i = 0, int ii = -1, string[] ldata = null)
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
            return true;
        }
        private void InitHeaders()
        {
            headers = properties.AsHeaders();
        }
        public void CleanHeaders()
        {
            if (headers is not null)
                Array.Clear(headers, 0, headers.Length);
        }
    }
}
