using CsvFileHandler.Handler;

using System.Collections.Immutable;
using System.Text;

namespace CsvFileHandler.IO;

public readonly struct LineData
{
    public LineData(string line, char delimiter)
    {
        var ld = line.Split(delimiter);
        FielData[] fd = new FielData[ld.Length];

        for (int i = 0; i < ld.Length; i++)        
            fd[i] = new FielData(i, ld[i]);
        
        Fields = [.. fd];
    }
    public readonly int Idx;
    public readonly ImmutableArray<FielData> Fields;
}
public readonly struct FielData(int idx, string data)
{
    public readonly int Idx = idx;
    public readonly string Data = data;
}

public sealed class CsvReader(string filePath, CsvReaderOptions options) : CsvHandler(filePath, options)
{
    public new CsvReaderOptions Options { get; set; } = options;
    public int ReadedLinesCount { get; private set; }
    public Dictionary<int, string> StoredLines { get; private set; } = null!;
    public int Lindex { get; private set; } = -1;
    public IEnumerable<string> ReadLines()
    {
        using FileStream strm = GetStream(StreamFileMode.ReadLines);
        using BufferedStream bfStrm = new BufferedStream(strm, Options.STREAM_BUFFER);
        using StreamReader rdr = new StreamReader(bfStrm, Encoding, leaveOpen: true);

        string? line;

        while ((line = rdr.ReadLine()) != null)
        {
            Lindex++;

            if (Options.IgnoreEmptyLines && string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            yield return line;
        }

        if (Options.StoreReadedLinesCount)
            ReadedLinesCount = Lindex + 1;
    }

    public IEnumerable<LineData> ReadLinesData()
    {
        using FileStream strm = GetStream(StreamFileMode.ReadLines);
        using BufferedStream bfStrm = new BufferedStream(strm, Options.STREAM_BUFFER);
        using StreamReader rdr = new StreamReader(bfStrm, Encoding, leaveOpen: true);

        string? line;

        while ((line = rdr.ReadLine()) != null)
        {
            Lindex++;

            if (Options.IgnoreEmptyLines && string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            yield return new LineData(line, Options.Delimiter);
        }

        if (Options.StoreReadedLinesCount)
            ReadedLinesCount = Lindex + 1;
    }

    public IEnumerable<string> ReadBlock(int fromLine, int toLine = -1, bool storeLines = false)
    {
        if (fromLine < 0 || (toLine != -1 && toLine < fromLine)) yield break;

        using FileStream strm = GetStream(StreamFileMode.ReadLines);
        using BufferedStream bfStrm = new BufferedStream(strm, Options.STREAM_BUFFER);
        using StreamReader rdr = new StreamReader(bfStrm, Encoding, leaveOpen: true);

        StringBuilder sb = new(Options.STREAM_BUFFER);

        if (storeLines)
            StoredLines = [];

        int lindex = -1;
        string? line;
        while ((line = rdr.ReadLine()) != null)
        {
            lindex++;

            if (Options.IgnoreEmptyLines && string.IsNullOrWhiteSpace(line))
                continue;

            if (lindex <= fromLine)
            {
                if (storeLines)
                    StoredLines.Add(lindex, line);
                continue;
            }

            if (toLine != -1 && lindex >= toLine)
            {
                if (storeLines)
                    StoredLines.Add(lindex, line);
                continue;
            }

            sb.Clear();
            sb.Append(line);
            yield return sb.ToString();
        }

        if (Options.StoreReadedLinesCount)
            ReadedLinesCount = lindex + 1;
    }
}
