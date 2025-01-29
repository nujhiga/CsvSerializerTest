using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvFileHandler.Handler;
public abstract class CsvHandler(string filePath, CsvHandlerOptions options)
{
    public string FilePath { get; set; } = filePath;
    public virtual CsvHandlerOptions Options { get; set; } = options;
    public char Delimiter => Options.Delimiter;
    public Encoding Encoding => Options.Encoding;
    protected byte[] GetStringBytes(string str) => Options.Encoding.GetBytes(str);
    protected FileStream GetStream(StreamFileMode fileHandleMode)
    {
        ArgumentNullException.ThrowIfNull(Options, nameof(Options));

        Options.SetStreamFileMode(fileHandleMode);
        return GetStream();
    }
    private FileStream GetStream()
    {
        ArgumentNullException.ThrowIfNull(Options.Encoding, nameof(Options.Encoding));
        ArgumentException.ThrowIfNullOrWhiteSpace(FilePath, nameof(FilePath));

        return new FileStream(FilePath, Options.FileMode, Options.FileAccess,
            Options.FileShare, Options.FILE_STREAM_BUFFER, Options.FileOptions);
    }
}
