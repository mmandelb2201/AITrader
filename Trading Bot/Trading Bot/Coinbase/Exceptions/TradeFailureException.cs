using System.Net;

namespace Trading_Bot.Coinbase.Exceptions;

public class TradeFailureException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    public TradeFailureException(HttpStatusCode statusCode, string responseContent)
        : base($"Trade failed with status code {statusCode}: {responseContent}")
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public TradeFailureException(string message, HttpStatusCode statusCode, string responseContent)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}