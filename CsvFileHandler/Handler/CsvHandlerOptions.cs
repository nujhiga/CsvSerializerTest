using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvFileHandler.Handler;

public enum StreamFileMode
{
    None,
    ReadLines,
    WriteLines,
    Deserialize,
}

public class CsvHandlerOptions(Encoding encoding, char delimiter)
{
    public readonly int FILE_STREAM_BUFFER = 4096;
    public readonly int STREAM_BUFFER = 8192;

    public Encoding Encoding { get; set; } = encoding;
    public char Delimiter { get; set; } = delimiter;

    public FileMode FileMode { get; set; }
    public FileAccess FileAccess { get; set; }
    public FileShare FileShare { get; set; }
    public FileOptions FileOptions { get; set; }
    public void SetStreamFileMode(StreamFileMode fileHandleMode)
    {
        switch (fileHandleMode)
        {
            case StreamFileMode.None:
                break;
            case StreamFileMode.ReadLines:
                FileMode = FileMode.Open;
                FileAccess = FileAccess.Read;
                FileShare = FileShare.Read;
                FileOptions = FileOptions.SequentialScan;
                break;
            case StreamFileMode.WriteLines:
                FileMode = FileMode.Create;
                FileAccess = FileAccess.Write;
                FileShare = FileShare.None;
                FileOptions = FileOptions.SequentialScan;
                break;
            default:
                break;
        }
    }
}
