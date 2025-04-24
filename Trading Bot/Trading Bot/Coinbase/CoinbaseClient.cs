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
        /// Retrieves detailed product information for the specified symbol from the Coinbase Advanced Trade API.
        /// </summary>
        /// <param name="symbol">The product symbol (e.g., "BTC-USD").</param>
        /// <returns>The deserialized <see cref="ProductDetails"/> object.</returns>
        /// <exception cref="ProductNotFoundException">Thrown if the product could not be found or the response could not be deserialized.</exception>
        public async Task<ProductDetails> GetProductAsync(string symbol)
        {
            using var client = new HttpClient();
            var requestUrl = $"{BaseURL}v3/brokerage/market/products/{symbol}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var details = JsonSerializer.Deserialize<ProductDetails>(json);
                if (details is null)
                    throw new ProductNotFoundException($"Unable to find product: {symbol}");

                return details;
            }
            catch (JsonException)
            {
                throw new ProductNotFoundException($"Unable to deserialize product: {symbol}");
            }
        }

        
        /// <summary>
        /// Grabs the current price of a given commodity and a given time.
        /// </summary>
        /// <param name="symbol">Commodity to search for.</param>
        /// <param name="time">Specified time to find the price for.</param>
        /// <returns>Price of commodity at given time.</returns>
        /// <exception cref="NoTradesFoundException">Throws if no price is found for given time.</exception>
        public async Task<Trade> GetProductAtTimeAsync(string symbol, long time)
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

        /// <summary>
        /// Gets user accounts associated with given Bearer token.
        /// </summary>
        /// <param name="bearerToken">Bearer token for current user.</param>
        /// <returns><see cref="AccountsResponse"/>> associated with given token.</returns>
        /// <exception cref="ArgumentNullException"> thrown if given token is null or empty.</exception>
        /// <exception cref="NoAccountsFoundException"> thrown if no accounts were found.</exception>
        public async Task<AccountsResponse> GetAccountsAsync(string bearerToken)
        {
            if (string.IsNullOrEmpty(bearerToken))
                throw new ArgumentNullException(nameof(bearerToken));
            
            using var client = new HttpClient();
            var requestUrl = BaseURL + "v3/brokerage/accounts";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var accountsResponse = JsonSerializer.Deserialize<AccountsResponse>(json);
            
            if (accountsResponse is null || accountsResponse.Accounts.Count == 0)
                throw new NoAccountsFoundException("GetAccounts returned 0 accounts.");
            
            return accountsResponse;
        }
        
        /// <summary>
        /// Places a limit order on the Coinbase Advanced Trade platform using the specified parameters.
        /// </summary>
        /// <param name="bearerToken">Bearer token for current user.</param>
        /// <param name="symbol">The trading pair symbol (e.g., "ETH-USD").</param>
        /// <param name="isBuy">True to place a BUY order; false to place a SELL order.</param>
        /// <param name="size">The integer quantity of the base currency to trade.</param>
        /// <param name="limitPrice">The limit price at which to place the order.</param>
        /// <returns>The raw JSON response from the Coinbase API as a string.</returns>
        /// <exception cref="TradeFailureException">Thrown when the order request fails or is rejected by the API.</exception>
        public async Task<string> CreateLimitOrderAsync(string bearerToken, string symbol, bool isBuy, int size, int limitPrice)
        {
            using var client = new HttpClient();
            var requestUrl = BaseURL + "v3/brokerage/orders";

            var orderPayload = new
            {
                client_order_id = Guid.NewGuid().ToString(),
                product_id = symbol,
                side = isBuy ? "BUY" : "SELL",
                order_configuration = new
                {
                    limit_limit_gtc = new
                    {
                        base_size = size.ToString(),           // fixed amount of ETH to trade
                        limit_price = limitPrice.ToString("F2"),   // formatted as decimal string (e.g., "3175.00")
                        post_only = true                        // ensures you’re a maker (won’t match instantly)
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Content = new StringContent(JsonSerializer.Serialize(orderPayload));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new TradeFailureException(response.StatusCode, result);
            }

            return result;
        }
    }
}
