using Trading_Bot.Config;

namespace Trading_Bot.Trader;

/// <summary>
/// TradeSizeCalculator uses a slightly modified Kelly Criterion to get a fraction
/// which corresponds with what percent of a wallet to buy or sell with.
/// </summary>
public static class TradeSizeCalculator
{
    // p = 0.5 is a neutral starting point as there's no historical data to estimate p with yet
    private const float WinPropbability = 0.5f;
    private const float AvgLoss = 0.0009f;
    private const float MaxAllocation = 0.2f;
    
    /// <summary>
    /// Gets what fraction of current portfolio should be traded.
    /// </summary>
    /// <param name="currentPrice">Most recent price of ETH.</param>
    /// <param name="predictedPrice">Predicted price of ETH.</param>
    /// <returns>Fraction of portfolio to trade.</returns>
    public static double GetTradeFraction(float currentPrice, float predictedPrice)
    {
        var expectedGain = (Math.Abs(predictedPrice) - currentPrice) / currentPrice;
        var b = expectedGain / AvgLoss;
        var fraction = ((WinPropbability * b) - (1- WinPropbability))/b;
        var reducedFraction = Configuration.RiskTolerance * fraction;
        return Math.Min(reducedFraction, MaxAllocation);
    }
}