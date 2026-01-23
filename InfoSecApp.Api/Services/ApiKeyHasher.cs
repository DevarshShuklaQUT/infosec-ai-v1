using System.Security.Cryptography;
using System.Text;

namespace InfoSecApp.Api.Services;

public static class ApiKeyHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 10000;

    /// <summary>
    /// Hashes an API key with a random salt using PBKDF2
    /// </summary>
    /// <param name="apiKey">The plain text API key to hash</param>
    /// <returns>Base64 encoded hash with salt (format: salt:hash)</returns>
    public static string HashApiKey(string apiKey)
    {
        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        // Hash the API key with the salt
        byte[] hash = HashWithSalt(apiKey, salt);
        
        // Combine salt and hash, then encode as Base64
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies an API key against a stored hash
    /// </summary>
    /// <param name="apiKey">The plain text API key to verify</param>
    /// <param name="hashedApiKey">The stored hash (format: salt:hash)</param>
    /// <returns>True if the key matches, false otherwise</returns>
    public static bool VerifyApiKey(string apiKey, string hashedApiKey)
    {
        try
        {
            // Split the stored hash into salt and hash parts
            var parts = hashedApiKey.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            // Hash the provided API key with the stored salt
            byte[] computedHash = HashWithSalt(apiKey, salt);

            // Compare the hashes using constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] HashWithSalt(string apiKey, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            apiKey,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }

    /// <summary>
    /// Generates a new random API key
    /// </summary>
    /// <param name="length">Length of the API key (default: 32)</param>
    /// <returns>A random API key string</returns>
    public static string GenerateApiKey(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        var result = new StringBuilder(length);
        
        foreach (byte b in randomBytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        
        return result.ToString();
    }
}
