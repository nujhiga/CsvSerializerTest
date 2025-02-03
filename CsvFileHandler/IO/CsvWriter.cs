using CsvFileHandler.Handler;

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CsvFileHandler.IO;
public sealed class CsvWriter(string filePath, CsvHandlerOptions options) : CsvHandler(filePath, options)
{
    public Queue<byte[]> QBuffer { get; private set; } = [];
    public void InitQBuffer()
    {
        QBuffer = new Queue<byte[]>();
    }
    public void SetBufferString(StringBuilder sb)
    {
        byte[] bytes = GetStringBytes(sb.ToString());
        if (bytes.Length == 0) return;

        QBuffer.Enqueue(bytes);
        sb.Clear();
    }

    public bool WriteBuffer()
    {
        using FileStream stream = GetStream(StreamFileMode.WriteLines);

        if (QBuffer.Count == 0) return false;

        while (QBuffer.TryDequeue(out var buffer))
            stream.Write(buffer, 0, buffer.Length);

        stream.Flush();

        QBuffer.Clear();
        QBuffer.TrimExcess();

        return true;
    }
}

public static class CsvWriterExtensions
{
    public static string ToCsvString<T>(this T obj, char separator, bool getHeaders = false)
    {
        if (obj is null) return string.Empty;

        StringBuilder sb = new();

        PropertyInfo[] properties = typeof(T).GetProperties();
        object[] objData = new object[properties.Length];

        if (!getHeaders)
        {
            for (int i = 0; objData.Length > i; i++)
            {
                objData[i] = properties[i].GetValue(obj)!;

                if (objData[i] is null)
                    objData[i] = string.Empty;
            }
        }
        else
        {
            for (int i = 0; objData.Length > i; i++)
                objData[i] = properties[i].Name;
        }

        return sb.AppendJoin(separator, objData).ToString();
    }
}
