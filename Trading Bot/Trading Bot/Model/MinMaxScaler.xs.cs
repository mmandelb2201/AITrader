namespace Trading_Bot.Model;

/// <summary>
/// <see cref="MinMaxScaler"/> is a c# implementation of the MinMaxScaler from the sklearn.preprocessing python package.
/// Performs MinMax Scaling on a set of data.
/// </summary>
public static class MinMaxScaler
{
    private static decimal min;
    private static decimal max;

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
    /// <param name="data">Array of decimal to scale.</param>
    /// <returns>Array of scaled data.</returns>
    public static decimal[] Transform(decimal[] data)
    {
        return data.Select(x => (x - min) / (max - min)).ToArray();
    }

    /// <summary>
    /// Descales data based on MinMax formula.
    /// </summary>
    /// <param name="data">Scaled decimal to descale.</param>
    /// <returns>Descaled number.</returns>
    public static decimal DeTransform(decimal data)
    {
        return (max - min) * data + min;
    }
}
