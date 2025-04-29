namespace Trading_Bot.Trader.Exceptions;

public class ApiKeyNullException() : Exception("Cannot find required API key name or secret. Please ensure .env file exists and configuration points to correct locaiton.")
{
}