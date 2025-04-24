using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Trading_Bot.Coinbase.Models
{
    public class ProductDetails
    {
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("price_percentage_change_24h")]
        public string PricePercentageChange24h { get; set; }

        [JsonPropertyName("volume_24h")]
        public string Volume24h { get; set; }

        [JsonPropertyName("volume_percentage_change_24h")]
        public string VolumePercentageChange24h { get; set; }

        [JsonPropertyName("base_increment")]
        public string BaseIncrement { get; set; }

        [JsonPropertyName("quote_increment")]
        public string QuoteIncrement { get; set; }

        [JsonPropertyName("quote_min_size")]
        public string QuoteMinSize { get; set; }

        [JsonPropertyName("quote_max_size")]
        public string QuoteMaxSize { get; set; }

        [JsonPropertyName("base_min_size")]
        public string BaseMinSize { get; set; }

        [JsonPropertyName("base_max_size")]
        public string BaseMaxSize { get; set; }

        [JsonPropertyName("base_name")]
        public string BaseName { get; set; }

        [JsonPropertyName("quote_name")]
        public string QuoteName { get; set; }

        [JsonPropertyName("watched")]
        public bool Watched { get; set; }

        [JsonPropertyName("is_disabled")]
        public bool IsDisabled { get; set; }

        [JsonPropertyName("new")]
        public bool New { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("cancel_only")]
        public bool CancelOnly { get; set; }

        [JsonPropertyName("limit_only")]
        public bool LimitOnly { get; set; }

        [JsonPropertyName("post_only")]
        public bool PostOnly { get; set; }

        [JsonPropertyName("trading_disabled")]
        public bool TradingDisabled { get; set; }

        [JsonPropertyName("auction_mode")]
        public bool AuctionMode { get; set; }

        [JsonPropertyName("product_type")]
        public string ProductType { get; set; }

        [JsonPropertyName("quote_currency_id")]
        public string QuoteCurrencyId { get; set; }

        [JsonPropertyName("base_currency_id")]
        public string BaseCurrencyId { get; set; }

        [JsonPropertyName("fcm_trading_session_details")]
        public object FcmTradingSessionDetails { get; set; }

        [JsonPropertyName("mid_market_price")]
        public string MidMarketPrice { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        [JsonPropertyName("alias_to")]
        public List<string> AliasTo { get; set; }

        [JsonPropertyName("base_display_symbol")]
        public string BaseDisplaySymbol { get; set; }

        [JsonPropertyName("quote_display_symbol")]
        public string QuoteDisplaySymbol { get; set; }

        [JsonPropertyName("view_only")]
        public bool ViewOnly { get; set; }

        [JsonPropertyName("price_increment")]
        public string PriceIncrement { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("product_venue")]
        public string ProductVenue { get; set; }

        [JsonPropertyName("approximate_quote_24h_volume")]
        public string ApproximateQuote24hVolume { get; set; }

        [JsonPropertyName("new_at")]
        public DateTime NewAt { get; set; }
    }
}