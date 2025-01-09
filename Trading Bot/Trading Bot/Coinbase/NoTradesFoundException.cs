namespace Trading_Bot.Coinbase
{
    internal class NoTradesFoundException(string message) : Exception(message)
    {
    }
}
