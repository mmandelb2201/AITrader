using Trading_Bot.Coinbase;
using Trading_Bot.Coinbase.Exceptions;
using Trading_Bot.Config;
using Trading_Bot.Model;

namespace Trading_Bot;

/// <summary>
/// <see cref="Predictor"/> Handles the entire prediction sequence, from getting current ETH values to invoking the ONNX LSTM.
/// </summary>
public static class Predictor
{
    private const string ETH = "ETH-USD";
    
    /// <summary>
    /// Grabs the 10 most recent prices of ETH, with 15 second gaps in between.
    /// Invokes an LSTM model to predict the price of ETH in 1 minute and 15 seconds from now.
    /// </summary>
    public static async Task PredictAsync()
    {
        var previousPrices = await GetPreviousPricesAsync().ConfigureAwait(false);
        var scaledPrices = MinMaxScaler.Transform(previousPrices);

        var predictedPrice = ModelInvoker.Predict(scaledPrices);
        Console.WriteLine($"Predicted price: {MinMaxScaler.DeTransform(predictedPrice)}");
    }

    private static async Task<float[]> GetPreviousPricesAsync()
    {
        var interval = Configuration.Interval;
        var sequenceLength = Configuration.SequenceLength;
        var prices = new List<float>();
        
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var coinbaseClient = new CoinbaseClient();
        for (int i = 0; i < sequenceLength; i++)
        {
            try
            {
                var trade = await coinbaseClient.GetPriceAsync(ETH, unixTime).ConfigureAwait(false);
                prices.Add(Convert.ToSingle(trade.Price));
            }
            catch (NoTradesFoundException e)
            {
                unixTime--;   
                var trade = await coinbaseClient.GetPriceAsync(ETH, unixTime).ConfigureAwait(false);
                prices.Add(Convert.ToSingle(trade.Price));
            }
            unixTime -= interval;
        }
        return prices.ToArray();
    }
}