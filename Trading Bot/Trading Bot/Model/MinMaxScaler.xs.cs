namespace Trading_Bot.Model;

/// <summary>
/// <see cref="MinMaxScaler"/> is a c# implementation of the MinMaxScaler from the sklearn.preprocessing python package.
/// Performs MinMax Scaling on a set of data.
/// </summary>
public static class MinMaxScaler
{
    private static float min;
    private static float max;

    /// <summary>
    /// Reads price column from a given csv file, and configures <see cref="MinMaxScaler"/> based on it.
    /// </summary>
    /// <param name="path">Path for csv file.</param>
    public static void Fit(string path)
    {
        var data = CsvReader.GetValuesFromColumn(path);
        min = data.Min();
        max = data.Max();
    }

    /// <summary>
    /// Performs MinMax scaling on set of data. Scale is set by Fit method.
    /// </summary>
    /// <param name="data">Array of float to transform.</param>
    /// <returns>Array of transformed data.</returns>
    public static float[] Transform(float[] data)
    {
        return data.Select(x => (x - min) / (max - min)).ToArray();
    }

    /// <summary>
    /// Descales data based on MinMax formula.
    /// </summary>
    /// <param name="data">Transformed float to detransform.</param>
    /// <returns>Detransformed number.</returns>
    public static float DeTransform(float data)
    {
        return (max - min) * data + min;
    }
}
