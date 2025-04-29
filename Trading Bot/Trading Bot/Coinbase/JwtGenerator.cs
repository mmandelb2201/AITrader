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
using System.Text.RegularExpressions;
using DotNetEnv;

namespace Trading_Bot.Coinbase;

/// <summary>
/// Handle JWT Token Generation For Coinbase APIs.
/// Implementation based on https://docs.cdp.coinbase.com/api/docs/authentication
/// </summary>
public static class JwtGenerator
{
        public static string Generate(string envPath)
        {
            Env.Load(envPath);
            
            // Fetching environment variables directly
            string name = Environment.GetEnvironmentVariable("KEY_NAME");
            string cbPrivateKey = Environment.GetEnvironmentVariable("KEY_SECRET");
            string requestMethod = Environment.GetEnvironmentVariable("REQUEST_METHOD") ?? "GET";
            string requestHost = Environment.GetEnvironmentVariable("REQUEST_HOST") ?? "api.coinbase.com";
            string requestPath = Environment.GetEnvironmentVariable("REQUEST_PATH") ?? "/api/v3/brokerage/products";

            // Validate that all necessary environment variables are provided
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cbPrivateKey) ||
                string.IsNullOrEmpty(requestMethod) || string.IsNullOrEmpty(requestHost) || string.IsNullOrEmpty(requestPath))
            {
                throw new InvalidOperationException("Missing required environment variables.");
            }

            string endpoint = requestMethod + " " + requestHost + requestPath;
            string token = GenerateToken(name, cbPrivateKey, endpoint);
            return token;
        }

        private static string GenerateToken(string name, string privateKeyPem, string uri)
        {
            // Load EC private key using BouncyCastle
            var ecPrivateKey = LoadEcPrivateKeyFromPem(privateKeyPem);

            // Create security key from the manually created ECDsa
            var ecdsa = GetECDsaFromPrivateKey(ecPrivateKey);
            var securityKey = new ECDsaSecurityKey(ecdsa);

            // Signing credentials
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);
            var now = DateTimeOffset.UtcNow;

            // Header and payload
            var header = new JwtHeader(credentials);
            header["kid"] = name;
            header["nonce"] = GenerateNonce(); // Generate dynamic nonce

            var payload = new JwtPayload
            {
                { "iss", "coinbase-cloud" },
                { "sub", name },
                { "nbf", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(2).ToUnixTimeSeconds() },
                { "uri", uri }
            };

            var token = new JwtSecurityToken(header, payload);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        // Method to generate a dynamic nonce
        private static string GenerateNonce(int length = 64)
        {
            byte[] nonceBytes = new byte[length / 2]; // Allocate enough space for the desired length (in hex characters)
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }
            return BitConverter.ToString(nonceBytes).Replace("-", "").ToLower(); // Convert byte array to hex string
        }

        // Method to load EC private key from PEM using BouncyCastle
        private static ECPrivateKeyParameters LoadEcPrivateKeyFromPem(string privateKeyPem)
        {
            using (var stringReader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(stringReader);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                    throw new InvalidOperationException("Failed to load EC private key from PEM");

                return (ECPrivateKeyParameters)keyPair.Private;
            }
        }

        // Method to convert ECPrivateKeyParameters to ECDsa
        private static ECDsa GetECDsaFromPrivateKey(ECPrivateKeyParameters privateKey)
        {
            var q = privateKey.Parameters.G.Multiply(privateKey.D).Normalize();
            var qx = q.AffineXCoord.GetEncoded();
            var qy = q.AffineYCoord.GetEncoded();

            var ecdsaParams = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256, // Adjust if you're using a different curve
                Q =
                {
                    X = qx,
                    Y = qy
                },
                D = privateKey.D.ToByteArrayUnsigned()
            };

            return ECDsa.Create(ecdsaParams);
        }
        
        public static bool IsJwtExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var exp = jwtToken.Payload.Exp; // seconds since epoch
            if (exp == null)
            {
                Console.WriteLine("No exp claim found.");
                return true;
            }

            var expiration = DateTimeOffset.FromUnixTimeSeconds((long)exp).UtcDateTime;
            return DateTime.UtcNow > expiration;
        }
}