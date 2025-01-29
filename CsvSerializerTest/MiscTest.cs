using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvSerializerTest;
public enum RecipeType
{
    None,
    RecipeN,
    RecipeA,
    RecipeB,
    RecipeC
}

public sealed class BuyOrder
{
    public int OrderID { get; set; }
    public int ClientID { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public RecipeType RecipeType { get; set; }
}

public static class MiscTest
{
    public static List<BuyOrder> CreateOrderLists(int count)
    {
        var rnd = new Random();
        var orders = new List<BuyOrder>(count);
        int rndval = 0;

        for (int i = 0; i < count; i++)
        {
            int orderID = i + 1;

            rndval = rnd.Next(1500, 25000);
            int clientID = rndval;

            rndval = rnd.Next(10, 25);
            string? description = setNull() ? null : GetRndString(rndval);

            DateTime? date = GetRndDateTime(ref rnd);

            rndval = rnd.Next(5000, 50000);
            int productID = rndval;

            rndval = rnd.Next(10, 1000);
            int quantity = rndval;

            decimal unitPrice = GetRndDecimal(ref rnd);

            rndval = rnd.Next(1, 4);
            RecipeType rtype = (RecipeType)rndval;

            orders.Add(new BuyOrder()
            {
                OrderID = orderID,
                ClientID = clientID,
                Description = description,
                CreatedDate = date,
                ProductID = productID,
                Quantity = quantity,
                UnitPrice = unitPrice,
                RecipeType = rtype
            });
        }
        return orders;
        bool setNull() => rnd.Next(0, 2) == 0;
    }
    private static decimal GetRndDecimal(ref Random rnd)
    {
        int d1 = rnd.Next(100, 3500);
        int d2 = rnd.Next(11, 99);
        string decStr = $"{d1},{d2}";
        return decimal.Parse(decStr);
    }
    private static DateTime? GetRndDateTime(ref Random rnd)
    {
        var day = rnd.Next(1, 25);
        var month = rnd.Next(1, 12);
        var year = rnd.Next(2020, 2025);
        string dateStr = $"{day}/{month}/{year}";

        DateTime? date = DateTime.Parse(dateStr);
        return date;
    }
    private static string GetRndString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];

        for (int i = 0; i < length; i++)
            stringChars[i] = chars[random.Next(chars.Length)];

        return new string(stringChars);
    }


    public static void Measure(Action action)
    {
        var stp = new Stopwatch();
        stp.Start();        
        action.Invoke();
        stp.Stop();
        Console.WriteLine($"{action.GetType().Name} => {stp.ElapsedMilliseconds}");
    }
}