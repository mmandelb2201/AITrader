using Trading_Bot.Coinbase;
using Trading_Bot.Coinbase.Exceptions;
using Trading_Bot.Config;
using Trading_Bot.Model;

namespace Trading_Bot.Trader;

public static class TradingSequence
{
    private const string ETH = "ETH-USD";

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
        return predictedPrice;
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