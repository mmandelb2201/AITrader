namespace Trading_Bot.Coinbase.Exceptions
{
    internal class NoTradesFoundException(string message) : Exception(message)
    {
    }
}
