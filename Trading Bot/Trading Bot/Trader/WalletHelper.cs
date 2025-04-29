using Trading_Bot.Coinbase;
using Trading_Bot.Coinbase.Models;
using Trading_Bot.Config;

namespace Trading_Bot.Trader;

/// <summary>
/// Helper methods to get data about user wallets.
/// </summary>
public class WalletHelper
{
    private const string UsdWalletName = "Cash (USD)";
    private const string CoinbaseRequestPath = "/api/v3/brokerage/accounts";
    private const string CoinbaseRequestMethod = "GET";
    private readonly string? _envFilePath = Configuration.EnvFilePath;
    private string _jwtToken;
    private readonly CoinbaseClient _coinbaseClient = new CoinbaseClient();

    /// <summary>
    /// Constructor for <see cref="WalletHelper"/>
    /// </summary>
    public WalletHelper()
    {
    }
    
    /// <summary>
    /// Constructor for <see cref="WalletHelper"/>
    /// </summary>
    /// <param name="envFilePath">Relative file path for .env file containing coinbase account information.</param>
    public WalletHelper(string? envFilePath)
    {
        _envFilePath = envFilePath;
    }

    /// <summary>
    /// Get wallet balance for specific coin.
    /// </summary>
    /// <param name="symbol"> coin to get balance for.</param>
    /// <returns>Amount of given coin user has.</returns>
    public async Task<decimal> GetBalanceAsync(string symbol)
    {
        var account = await GetAccountForSymbolAsync(symbol).ConfigureAwait(false);
        return account.AvailableBalance.DecimalValue;
    }

    /// <summary>
    /// Get user account which holds given symbol
    /// </summary>
    /// <param name="symbol"> symbol to get account for.</param>
    /// <returns><see cref="Account"/> for given symbol.</returns>
    public async Task<Account> GetAccountForSymbolAsync(string symbol)
    {
        if (string.IsNullOrEmpty(_jwtToken) || JwtGenerator.IsJwtExpired(_jwtToken))
        {
            _jwtToken = JwtGenerator.Generate(_envFilePath, CoinbaseRequestMethod, CoinbaseRequestPath);
        }

        var accountsResponse = await _coinbaseClient.GetAccountsAsync(_jwtToken).ConfigureAwait(false);
        return accountsResponse.Accounts.First(a => a.Currency == symbol);
    }

    public async Task<Account> GetUsdAccountAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken) || JwtGenerator.IsJwtExpired(_jwtToken))
        {
            _jwtToken = JwtGenerator.Generate(_envFilePath, CoinbaseRequestMethod, CoinbaseRequestPath);
        }
        
        var accountsResponse = await _coinbaseClient.GetAccountsAsync(_jwtToken).ConfigureAwait(false);
        return accountsResponse.Accounts.First(a => a.Name.Equals(UsdWalletName, StringComparison.InvariantCultureIgnoreCase)); 
    }
}