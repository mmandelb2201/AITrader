using Trading_Bot.Coinbase;
using Trading_Bot.Coinbase.Exceptions;
using Trading_Bot.Config;
using Trading_Bot.Logger;
using Trading_Bot.Model;

namespace Trading_Bot.Trader;

/// <summary>
/// Performs entire sequence to see how much to trade, and whether to buy or sell.
/// </summary>
public static class TradingSequence
{
    private const string ETH = "ETH-USD";
    private static readonly PriceLogger _priceLogger = new PriceLogger();
    private static readonly PredictionLogger _predictionLogger = new PredictionLogger();

    /// <summary>
    /// Invokes LSTM Model to grab predicted price, and calculates what percent of portforlio to trade.
    /// </summary>
    /// <returns>Tuple containing a bool and a double. First value is <see langword="true" /> if price will go up,
    /// <see langword="false" /> otherwise. Second value is the predicted price.</returns>
    public static async Task<(bool, double)> TradingStepAsync()
    {
        var previousPrices = await GetPreviousPricesAsync().ConfigureAwait(false);
        var predictedPrice = Predict(previousPrices);
        var descaledPrice = MinMaxScaler.DeTransform(predictedPrice);
        Console.WriteLine($"Predicted price: {descaledPrice}");
        var portfolioFraction = TradeSizeCalculator.GetTradeFraction(previousPrices[9], descaledPrice);
        Console.WriteLine($"Portfolio fraction: {portfolioFraction}");
        bool isBuy = descaledPrice > portfolioFraction;
        return (isBuy, portfolioFraction);
    }
    
    private static float Predict(float[] inputs)
    {
        var scaledPrices = MinMaxScaler.Transform(inputs);
        var predictedPrice = ModelInvoker.Predict(scaledPrices);
        //Log prediction as this helps check model performance with real data.
        _predictionLogger.LogPrice(MinMaxScaler.DeTransform(predictedPrice));
        return predictedPrice;
    }

    private static async Task<float[]> GetPreviousPricesAsync()
    {
        var interval = Configuration.Interval;
        var sequenceLength = Configuration.SequenceLength;
        var prices = new List<float>();
        
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var coinbaseClient = new CoinbaseClient();
        while (prices.Count < Configuration.SequenceLength)
        {
            try
            {
                var trade = await coinbaseClient.GetPriceAsync(ETH, unixTime).ConfigureAwait(false);
                prices.Add(Convert.ToSingle(trade.Price));

                //Log prices. This helps retraining the model with more datapoints later.
                _priceLogger.LogPrice(DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime, Convert.ToSingle(trade.Price));
                unixTime -= interval;
            }
            catch (NoTradesFoundException)
            {
                unixTime--;
            }
        }
        return prices.ToArray();
    }
}