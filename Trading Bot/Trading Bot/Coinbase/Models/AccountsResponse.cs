using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models;

public class AccountsResponse
{
    [JsonPropertyName("accounts")]
    public List<Account> Accounts { get; set; }

    [JsonPropertyName("has_next")]
    public bool HasNext { get; set; }

    [JsonPropertyName("cursor")]
    public string Cursor { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}