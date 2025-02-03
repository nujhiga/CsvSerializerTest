using CsvFileHandler.Handler;
using CsvFileHandler.IO;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CsvSerializer;
public sealed class Serializer : CsvSerializationBase
{
    public Serializer(string filePath, char delimiter, Encoding encoding, SerializationOptions options) : base(options)
    {
        CsvHandlerOptions hoptions = new(encoding, delimiter);
        writer = new CsvWriter(filePath, hoptions);
    }

    private readonly NumberFormatInfo nfi = new NumberFormatInfo
    {
        NumberDecimalSeparator = "."
    };

    internal readonly CsvWriter writer;
    private int valLen = 0;

    public bool Serialize<T>(CsvCollection<T> csvColl) where T : class, new()
    {
        return Serialize(csvColl.AsEnumerable());
    }
    public bool Serialize<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        var del = writer.Delimiter;
        InitializeProperties<T>(del);

        writer.InitQBuffer();
        StringBuilder sb = new();

        object[] values = GetValues(obj);

        if (Properties.RequireFormat)
        {
            AppendValuesFormat(sb, values, del);
        }
        else
        {
            AppendValues(sb, values, del);
        }
        writer.SetBufferString(sb);
        writer.WriteBuffer();
        return true;
    }
    public bool Serialize<T>(IEnumerable<T> objs) where T : class
    {
        ArgumentNullException.ThrowIfNull(objs);
        var del = writer.Delimiter;
        InitializeProperties<T>(del);

        writer.InitQBuffer();
        StringBuilder sb = new();

        if (Options.Mode is SerializationMode.Header)
            _ = sb.AppendJoin(writer.Delimiter, Properties.AsHeaders()).Append(Environment.NewLine);

        foreach (var obj in objs)
        {
            if (obj is null) continue;

            object[] values = GetValues(obj);

            if (Properties.RequireFormat)
            {
                AppendValuesFormat(sb, values, del);
            }
            else
            {
                AppendValues(sb, values, del);
            }

            if (sb.Length > writer.Options.FILE_STREAM_BUFFER)
                writer.SetBufferString(sb);
        }

        if (sb.Length > 0) writer.SetBufferString(sb);

        writer.WriteBuffer();

        return true;
    }

    private void AppendValues(StringBuilder sb, object[] values, char del)
    {
        for (int i = 0; i < valLen; i++)
        {
            if (i > 0) sb.Append(del);

            object val = values[i];
            if (val is string str)
            {
                _ = sb.Append(str);
            }
            else
            {
                _ = sb.Append(val?.ToString() ?? string.Empty);
            }
        }

        sb.Append(Environment.NewLine);
    }
    private void AppendValuesFormat(StringBuilder sb, object[] values, char del)
    {
        for (int i = 0; i < valLen; i++)
        {
            if (i > 0) sb.Append(del);
            AppendValueFormat(sb, values[i]);
        }

        _=sb.Append(Environment.NewLine);
    }
    private void AppendValueFormat(StringBuilder sb, object value)
    {
        if (value is null)
        {
            _ = sb.Append(string.Empty);
        }
        else if (value is decimal d)
        {
            _ = sb.Append(d.ToString(nfi));
        }
        else if (value is double db)
        {
            _ = sb.Append(db.ToString(nfi));
        }
        else if (value is float f)
        {
            _ = sb.Append(f.ToString(nfi));
        }
        else if (value is string s)
        {
            _ = sb.Append(s);
        }
        else
        {
            _ = sb.Append(value.ToString());
        }
    }
    private object[] GetValues<T>(T obj) where T : class
    {
        valLen = Properties.Count;
        object[] objs = new object[valLen];

        for (int i = 0; i < valLen; i++)
        {
            var prop = Properties[i];
            if (prop.Ignore) continue;
            objs[i] = prop.Pinfo.GetValue(obj)!;
        }

        return objs;

        /*
        Collection<object> objs = [];
        foreach (var item in Properties)
        {
            if (item.Ignore) continue;
            objs.Add(item.Pinfo.GetValue(obj));
        }
        return [.. objs];*/

        /*object[] objs = new object[Properties.Count];
        valuesLen = objs.Length;

        int i = 0;
        if (Options.Mode is CsvSerializationMode.Ordinal)
        {
            foreach (var prop in OrdinalProperties.Values)
                objs[i++] = prop.GetValue(obj)!;

            return objs;
        }

        foreach (var prop in HeaderProperties.Values)
            objs[i++] = prop.GetValue(obj)!;

        return objs;*/
    }

}
