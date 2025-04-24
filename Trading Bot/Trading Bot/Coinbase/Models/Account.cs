using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models;

public class Account
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("available_balance")]
    public Balance AvailableBalance { get; set; }

    [JsonPropertyName("default")]
    public bool Default { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    [JsonPropertyName("hold")]
    public Balance Hold { get; set; }

    [JsonPropertyName("retail_portfolio_id")]
    public string RetailPortfolioId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }
}