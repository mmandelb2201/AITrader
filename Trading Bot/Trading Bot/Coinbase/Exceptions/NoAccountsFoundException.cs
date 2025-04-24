namespace Trading_Bot.Coinbase.Exceptions;

public class NoAccountsFoundException(string message) : Exception(message)
{
}