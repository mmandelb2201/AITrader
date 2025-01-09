using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models
{
    public class Trade
    {
        [JsonPropertyName("trade_id")]
        public string TradeId { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }
    }
}
