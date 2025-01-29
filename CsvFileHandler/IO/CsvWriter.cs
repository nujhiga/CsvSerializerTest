using CsvFileHandler.Handler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvFileHandler.IO;
public sealed class CsvWriter(string filePath, CsvHandlerOptions options) : CsvHandler(filePath, options)
{
    public Queue<byte[]> QBuffer { get; private set; } = [];
    public void InitQBuffer()
    {
        QBuffer = new Queue<byte[]>();
    }
    public void SetBufferString(ref StringBuilder sb)
    {
        QBuffer.Enqueue(GetStringBytes(sb.ToString()));
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
        QBuffer = new Queue<byte[]>();

        return true;
    }
}
