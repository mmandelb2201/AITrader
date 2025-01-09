using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models
{
    public class RootObject
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("schema")]
        public Schema Schema { get; set; }
    }
}
