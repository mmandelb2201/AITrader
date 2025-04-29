namespace Trading_Bot.Model;

public static class DecimalFloatConverter
{
    /// <summary>
    /// Converts an array of decimals to an array of floats.
    /// Warning: This may lose precision because float has less precision than decimal.
    /// </summary>
    public static float[] DecimalArrayToFloatArray(decimal[] decimals)
    {
        if (decimals is null)
        {
            throw new ArgumentNullException(nameof(decimals));
        }

        float[] floats = new float[decimals.Length];
        for (int i = 0; i < decimals.Length; i++)
        {
            floats[i] = (float)decimals[i];
        }
        return floats;
    }

    /// <summary>
    /// Converts an array of floats to an array of decimals.
    /// </summary>
    public static decimal[] FloatArrayToDecimalArray(float[] floats)
    {
        if (floats is null)
        {
            throw new ArgumentNullException(nameof(floats));
        }

        decimal[] decimals = new decimal[floats.Length];
        for (int i = 0; i < floats.Length; i++)
        {
            decimals[i] = (decimal)floats[i];
        }
        return decimals;
    }
}
