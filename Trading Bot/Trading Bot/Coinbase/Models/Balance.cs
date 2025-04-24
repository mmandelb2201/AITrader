using System.Text.Json.Serialization;
using Sprache;

namespace Trading_Bot.Coinbase.Models;

public class Balance
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    public decimal DecimalValue => decimal.Parse(Value);
}