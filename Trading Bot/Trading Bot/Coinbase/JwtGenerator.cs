using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.IO;
using DotNetEnv;
using Trading_Bot.Config;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Trading_Bot.Coinbase;

/// <summary>
/// Handles JWT Token Generation for Coinbase APIs.
/// Implementation based on https://docs.cdp.coinbase.com/api/docs/authentication
/// </summary>
public static class JwtGenerator
{
    private static string _token;

    public static string Generate(string envPath = "", string requestMethod = "GET", string requestPath = "/api/v3/brokerage/products")
    {
        envPath = string.IsNullOrEmpty(envPath) ? Configuration.EnvFilePath : envPath;
        Env.Load(envPath);

        string name = Environment.GetEnvironmentVariable("KEY_NAME");
        string cbPrivateKey = Environment.GetEnvironmentVariable("KEY_SECRET");
        string requestHost = Environment.GetEnvironmentVariable("REQUEST_HOST") ?? "api.coinbase.com";

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cbPrivateKey) ||
            string.IsNullOrEmpty(requestMethod) || string.IsNullOrEmpty(requestHost) || string.IsNullOrEmpty(requestPath))
        {
            throw new InvalidOperationException("Missing required environment variables.");
        }

        string endpoint = $"{requestMethod} {requestHost}{requestPath}";
        return GenerateToken(name, cbPrivateKey, endpoint);
    }

    private static string GenerateToken(string name, string privateKeyPem, string uri)
    {
        var ecPrivateKey = LoadEcPrivateKeyFromPem(privateKeyPem);
        var ecdsa = GetECDsaFromPrivateKey(ecPrivateKey);
        var securityKey = new ECDsaSecurityKey(ecdsa);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);
        var now = DateTimeOffset.UtcNow;

        var header = new JwtHeader(credentials)
        {
            { "kid", name },
            { "nonce", GenerateNonce() }
        };

        var payload = new JwtPayload
        {
            { "iss", "coinbase-cloud" },
            { "sub", name },
            { "nbf", now.ToUnixTimeSeconds() },
            { "exp", now.AddMinutes(2).ToUnixTimeSeconds() },
            { "uri", uri }
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateNonce(int length = 64)
    {
        byte[] nonceBytes = new byte[length / 2];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }

        return BitConverter.ToString(nonceBytes).Replace("-", "").ToLower();
    }

    private static ECPrivateKeyParameters LoadEcPrivateKeyFromPem(string privateKeyPem)
    {
        using var stringReader = new StringReader(privateKeyPem);
        var pemReader = new PemReader(stringReader);
        var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;

        if (keyPair is null)
        {
            throw new InvalidOperationException("Failed to load EC private key from PEM.");
        }

        return (ECPrivateKeyParameters)keyPair.Private;
    }

    private static ECDsa GetECDsaFromPrivateKey(ECPrivateKeyParameters privateKey)
    {
        var q = privateKey.Parameters.G.Multiply(privateKey.D).Normalize();
        var qx = q.AffineXCoord.GetEncoded();
        var qy = q.AffineYCoord.GetEncoded();

        var dBytes = privateKey.D.ToByteArrayUnsigned();
        if (dBytes.Length < 32)
        {
            var padded = new byte[32];
            Array.Copy(dBytes, 0, padded, 32 - dBytes.Length, dBytes.Length);
            dBytes = padded;
        }

        var ecdsaParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = { X = qx, Y = qy },
            D = dBytes
        };

        return ECDsa.Create(ecdsaParams);
    }

    public static bool IsJwtExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var exp = jwtToken.Payload.Exp;
        if (exp is null)
        {
            Console.WriteLine("No exp claim found.");
            return true;
        }

        var expiration = DateTimeOffset.FromUnixTimeSeconds((long)exp).UtcDateTime;
        return DateTime.UtcNow > expiration;
    }
}
