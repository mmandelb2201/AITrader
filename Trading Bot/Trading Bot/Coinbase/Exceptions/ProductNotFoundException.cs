namespace Trading_Bot.Coinbase.Exceptions;

public class ProductNotFoundException(string message) : Exception(message)
{
}