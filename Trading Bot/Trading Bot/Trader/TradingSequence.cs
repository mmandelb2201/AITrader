using System.Net;
using DotNetEnv;
using Trading_Bot.Coinbase;
using Trading_Bot.Coinbase.Exceptions;
using Trading_Bot.Config;
using Trading_Bot.Logger;
using Trading_Bot.Model;
using Trading_Bot.Trader.Exceptions;

namespace Trading_Bot.Trader;

/// <summary>
/// Performs entire sequence to see how much to trade, and whEther to buy or sell.
/// </summary>
public static class TradingSequence
{
    private static readonly PriceLogger PriceLogger = new PriceLogger();
    private static readonly PredictionLogger PredictionLogger = new PredictionLogger();
    private static readonly string SymbolUsd = Configuration.Symbol + "-USD";

    /// <summary>
    /// Invokes LSTM Model to grab predicted price, and calculates what percent of portforlio to trade.
    /// </summary>
    /// <returns>Tuple containing a bool and a double. First value is <see langword="true" /> if price will go up,
    /// <see langword="false" /> otherwise. Second value is the predicted price.</returns>
    public static async Task PredictionStepAsync()
    {
        var previousPrices = await GetPreviousPricesAsync().ConfigureAwait(false);
        var predictedPrice = Predict(previousPrices);
        var descaledPrice = MinMaxScaler.DeTransform(predictedPrice);
        Console.WriteLine($"Predicted price: {descaledPrice}");
        
        var walletHelper = new WalletHelper();
        var usdWallet = await walletHelper.GetUsdAccountAsync().ConfigureAwait(false);
        var usdBalance = usdWallet.AvailableBalance.DecimalValue;
        var ethBalance = await walletHelper.GetBalanceAsync(Configuration.Symbol).ConfigureAwait(false);
        await TryPlaceTradeAsync(previousPrices[0], descaledPrice, usdBalance, ethBalance, Configuration.Symbol).ConfigureAwait(false);
    }
    
    private static decimal Predict(decimal[] inputs)
    {
        var scaledPrices = MinMaxScaler.Transform(inputs);
        var predictedPrice = ModelInvoker.Predict(scaledPrices);
        //Log prediction as this helps check model performance with real data.
        PredictionLogger.LogPrice(MinMaxScaler.DeTransform(predictedPrice));
        return predictedPrice;
    }

    private static async Task<decimal[]> GetPreviousPricesAsync()
    {
        var interval = Configuration.Interval;
        var prices = new List<decimal>();
        
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var coinbaseClient = new CoinbaseClient();
        while (prices.Count < Configuration.SequenceLength)
        {
            try
            {
                var trade = await coinbaseClient.GetProductAtTimeAsync(SymbolUsd, unixTime).ConfigureAwait(false);
                prices.Add(decimal.Parse(trade.Price));

                //Log prices. This helps retraining the model with more datapoints later.
                PriceLogger.LogPrice(DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime,
                    decimal.Parse(trade.Price));
                unixTime -= interval;
            }
            catch (NoTradesFoundException)
            {
                unixTime--;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("[Error] Too many requests. Retrying in 3 seconds...");
                await Task.Delay(3 * 1000).ConfigureAwait(false);
            }
        }
        return prices.ToArray();
    }
    
    private static async Task TryPlaceTradeAsync(
        decimal currentPrice,
        decimal predictedPrice,
        decimal usdBalance,
        decimal ethBalance,
        string symbol)
    {
        Console.WriteLine($"[INFO] Trying to place trade. Current Price: {FormatUsd(currentPrice)}. " +
                          $"Predicted Price: {FormatUsd(predictedPrice)}. Usd Balance: {FormatUsd(usdBalance)}." +
                          $" Eth Balance: {ethBalance} ETH." +
                          $" Symbol: {symbol}");
        
        decimal percentThreshold = Configuration.TradePercentageThreshold;
        decimal limitOffset = Configuration.LimitOffset;

        var priceDiff = predictedPrice - currentPrice;
        var percentDiff = priceDiff / currentPrice;

        using var coinbaseClient = new CoinbaseClient();
        
        if (percentDiff >= percentThreshold)
        {
            // BUY Eth with 20% of USD wallet
            decimal usdToUse = usdBalance * 0.20m;
            decimal ethToBuy = usdToUse / currentPrice;

            // Place slightly below current market price
            decimal limitPrice = Math.Max(0.01m, currentPrice - limitOffset);
            Console.WriteLine($"[BUY] Predicted ↑ {FormatPercent(percentDiff)}. Buying {ethToBuy:F6} Eth at {FormatUsd(limitPrice)}");
            
            Env.Load(Configuration.EnvFilePath);

            string name = Environment.GetEnvironmentVariable("KEY_NAME");
            string cbPrivateKey = Environment.GetEnvironmentVariable("KEY_SECRET");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cbPrivateKey))
            {
                throw new ApiKeyNullException();
            }
            
            await coinbaseClient.CreateLimitOrderAsync(
                SymbolUsd,
                isBuy: true,
                size: ethToBuy,
                limitPrice: limitPrice
            ).ConfigureAwait(false);
        }
        else if (percentDiff <= -percentThreshold)
        {
            // SELL Eth using 20% of Eth wallet
            decimal ethToSell = ethBalance * 0.20m;

            // Place slightly above market
            decimal limitPrice = currentPrice + limitOffset;
            Console.WriteLine($"[SELL] Predicted ↓ {FormatPercent(percentDiff)}. Selling {ethToSell:F6} Eth at {FormatUsd(limitPrice)}");

            Env.Load(Configuration.EnvFilePath);

            string name = Environment.GetEnvironmentVariable("KEY_NAME");
            string cbPrivateKey = Environment.GetEnvironmentVariable("KEY_SECRET");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cbPrivateKey))
            {
                throw new ApiKeyNullException();
            }
            
            await coinbaseClient.CreateLimitOrderAsync(
                SymbolUsd,
                isBuy: false,
                size: ethToSell,
                limitPrice: limitPrice
            ).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine($"[HOLD] No trade. Δ = {FormatPercent(percentDiff, 5)}.");
        }
    }

    private static string FormatUsd(decimal value) => $"${value:N2}";
    
    private static string FormatPercent(decimal value, int decimals = 1)
    {
        return value.ToString($"P{decimals}");
    }
}