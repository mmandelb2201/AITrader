using System;
using System.Linq;

namespace Trading_Bot.Model;

public static class MinMaxScaler
{
    private static float min;
    private static float max;

    // Fit the scaler by calculating the min and max of the data
    public static void Fit(string path)
    {
        var data = CsvReader.GetValuesFromColumn(path);
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"MinMaxScaler {i}: {data[i]}");
        }
        min = data.Min();
        max = data.Max();
    }

    // Transform the data using the fitted min and max
    public static float[] Transform(float[] data)
    {
        return data.Select(x => (x - min) / (max - min)).ToArray();
    }

    public static float DeTransform(float data)
    {
        return (max - min) * data + min;
    }
}
