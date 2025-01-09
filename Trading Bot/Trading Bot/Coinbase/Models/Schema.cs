using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models
{
    public class Schema
    {
        [JsonPropertyName("trades")]
        public List<Trade> Trades { get; set; }

        [JsonPropertyName("best_bid")]
        public string BestBid { get; set; }

        [JsonPropertyName("best_ask")]
        public string BestAsk { get; set; }
    }
}
