using CsvFileHandler.Handler;
using System.Text;

namespace CsvFileHandler.IO;
public sealed class CsvReaderOptions(Encoding encoding, char delimiter) : CsvHandlerOptions(encoding, delimiter)
{
    public bool IgnoreEmptyLines { get; set; } = true;
    public bool UseCurrentLindex { get; set; } = true;
    public bool StoreReadedLinesCount { get; set; } = false;
}
