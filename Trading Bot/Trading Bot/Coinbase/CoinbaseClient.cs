using System.Net.Http.Headers;
using System.Text.Json;
using Trading_Bot.Coinbase.Models;
using Trading_Bot.Coinbase.Exceptions;

namespace Trading_Bot.Coinbase
{
    /// <summary>
    /// Http client to connect with Coinbase REST API.
    /// </summary>
    internal class CoinbaseClient
    {
        private readonly string BaseURL = "https://api.coinbase.com/api/";
        /// <summary>
        /// Grabs the current price of a given commodity and a given time.
        /// </summary>
        /// <param name="symbol">Commodity to search for.</param>
        /// <param name="time">Specified time to find the price for.</param>
        /// <returns>Price of commodity at given time.</returns>
        /// <exception cref="NoTradesFoundException">Throws if no price is found for given time.</exception>
        public async Task<Trade> GetPriceAsync(string symbol, long time)
        {
            using var client = new HttpClient();
            var requestUrl = BaseURL + $"v3/brokerage/market/products/{symbol}/ticker?limit=1&start={time-1}&end={time}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var schema = JsonSerializer.Deserialize<Schema>(json);

            if (schema is null || schema.Trades.Count == 0)
                throw new NoTradesFoundException("GetPrice returned 0 trades.");

            return schema.Trades.First();
        }
    }
}
