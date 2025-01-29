using CsvFileHandler.IO;

using CsvSerializer;
using CsvSerializer.Filtering;

using CsvSerializerTest;

using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

//string fpath = $"D:\\Documents\\orders{33}.csv";

string fpath = $"D:\\Documents\\orderss.csv";

char del = ',';
Encoding encoding = Encoding.UTF8;


CsvReader reader = new(fpath, new CsvReaderOptions(encoding, del));

Collection<ImmutableArray<string>> lines = [];
ImmutableArray<LineData> ld;

MiscTest.Measure(testl);



void testl()
{
    ld = reader.ReadLinesData().ToImmutableArray();
}

Console.ReadKey();
{ }






SerializationOptions serOptions = new(SerializationMode.Ordinal, HeadersMode.OrdinalIgnore);
//serOptions.SetFilterNames(FilterMode.Ignore, 
//    nameof(BuyOrder.CreatedDate), nameof(BuyOrder.ProductID));

Deserializer des = new(fpath, del, encoding, serOptions);

BuyOrder[] borders = null!;
MiscTest.Measure(Test);

Console.ReadKey();
{ }


void Test() 
{
}


