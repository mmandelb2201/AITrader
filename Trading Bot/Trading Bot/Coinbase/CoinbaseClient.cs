using System.Net.Http.Headers;
using System.Text.Json;
using Trading_Bot.Coinbase.Models;

namespace Trading_Bot.Coinbase
{
    internal class CoinbaseClient
    {
        private readonly string BaseURL = "https://api.coinbase.com/api/";
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
            var rootObject = JsonSerializer.Deserialize<RootObject>(json);

            if (rootObject is null || rootObject.Schema.Trades.Count == 0)
                throw new NoTradesFoundException("GetPrice returned 0 trades");

            return rootObject.Schema.Trades.First();
        }
    }
}
