using System.Globalization;
using CsvHelper;

namespace Trading_Bot.Model;

internal static class CsvReader
{
    public static float[] GetValuesFromColumn(string path)
    {
        List<float> prices = new List<float>();

        using (var reader = new StreamReader(path))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                if (float.TryParse(record.price, out float price))
                {
                    prices.Add(price);
                }
            }
        }
        return prices.ToArray();
    }
}