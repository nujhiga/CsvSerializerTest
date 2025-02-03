using CsvSerializer;
using CsvSerializer.Filtering;

using CsvSerializerTest;

using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

//string fpath = $"D:\\Documents\\orders{33}.csv";
//string fpath = $"D:\\Documents\\orderss.csv";

string fpath = $"D:\\Documents\\orderss22.csv";

char del = ',';
Encoding encoding = Encoding.UTF8;

SerializationOptions serOptions = 
    new(SerializationMode.Header, 
        HeadersMode.FromType);

//serOptions.SetFilterNames(FilterMode.Ignore, 
//    nameof(BuyOrder.CreatedDate), nameof(BuyOrder.ProductID));

Deserializer des = new(fpath, del, encoding, serOptions);
Serializer ser = new(fpath, del, encoding, serOptions);

var lstord = MiscTest.CreateOrderLists(5000);
MiscTest.Measure(Test2);

Console.ReadKey();


{ }



void Test2()
{
    ser.Serialize(lstord);
}

BuyOrder[] orders = null!;
ConcurrentBag<BuyOrder> bagOrders;
CsvCollection<BuyOrder> cst = null!;
MiscTest.Measure(Test);

{ }

Console.ReadKey();

void Test() 
{
    //orders = des.Deserialize<BuyOrder>().ToArray();
    //bagOrders = des.ParallelDeserialize<BuyOrder>();
    cst = CsvCollectionFactory.CreateParallel<BuyOrder>(des, 1000000);
    //cst = CsvCollectionFactory.Create<BuyOrder>(des, 1000000);
    
}

