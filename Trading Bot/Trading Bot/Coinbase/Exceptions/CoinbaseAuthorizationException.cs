namespace Trading_Bot.Coinbase.Exceptions;

public class CoinbaseAuthorizationException() : Exception("Request was unauthorized. Please ensure your JWT token is valid.")
{
}