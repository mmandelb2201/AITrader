using System.Globalization;
using CsvHelper;

namespace Trading_Bot.Model;

internal static class CsvReader
{
    public static decimal[] GetValuesFromColumn(string path)
    {
        List<decimal> prices = new List<decimal>();

        using (var reader = new StreamReader(path))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                if (decimal.TryParse(record.price, out decimal price))
                {
                    prices.Add(price);
                }
            }
        }
        return prices.ToArray();
    }
}