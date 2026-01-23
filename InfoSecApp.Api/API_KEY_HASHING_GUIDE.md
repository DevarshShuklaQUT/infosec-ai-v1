# API Key Hasher Utility

## How to Generate Hashed API Keys

### Option 1: Using the API Project (Recommended)

Create a temporary file `HashKeys.cs` in the InfoSecApp.Api folder:

```csharp
using InfoSecApp.Api.Services;

// Generate hashed keys for your API keys
var keys = new[]
{
    "test-api-key-12345",
    "prod-api-key-67890"
};

Console.WriteLine("=== Generated Hashed API Keys ===\n");

foreach (var key in keys)
{
    var hashedKey = ApiKeyHasher.HashApiKey(key);
    Console.WriteLine($"Plain: {key}");
    Console.WriteLine($"Hash:  {hashedKey}");
    Console.WriteLine();
}

// Generate a new random API key
Console.WriteLine("=== New Random API Key ===\n");
var newKey = ApiKeyHasher.GenerateApiKey(32);
var newHashedKey = ApiKeyHasher.HashApiKey(newKey);
Console.WriteLine($"Plain: {newKey}");
Console.WriteLine($"Hash:  {newHashedKey}");
Console.WriteLine("\nIMPORTANT: Save the plain key securely!");
```

Then temporarily rename `Program.cs` to `Program.cs.bak`, rename `HashKeys.cs` to `Program.cs`, run `dotnet run`, then restore the original `Program.cs`.

### Option 2: Using C# Interactive (Quick Method)

```bash
cd InfoSecApp.Api
dotnet fsi
```

Then paste this code:

```fsharp
#r "System.Security.Cryptography.dll"
open System.Security.Cryptography
open System

let hashApiKey (apiKey: string) =
    let saltSize = 16
    let keySize = 32
    let iterations = 10000
    
    let salt = RandomNumberGenerator.GetBytes(saltSize)
    use pbkdf2 = new Rfc2898DeriveBytes(apiKey, salt, iterations, HashAlgorithmName.SHA256)
    let hash = pbkdf2.GetBytes(keySize)
    
    sprintf "%s:%s" (Convert.ToBase64String(salt)) (Convert.ToBase64String(hash))

// Hash your API keys
printfn "test-api-key-12345 -> %s" (hashApiKey "test-api-key-12345")
printfn "prod-api-key-67890 -> %s" (hashApiKey "prod-api-key-67890")
```

### Option 3: Manual C# Snippet

Create a new console app temporarily:

```bash
mkdir /tmp/ApiKeyHasher
cd /tmp/ApiKeyHasher
dotnet new console
```

Replace Program.cs content with:

```csharp
using System.Security.Cryptography;

var key1 = "test-api-key-12345";
var key2 = "prod-api-key-67890";

Console.WriteLine($"test-api-key-12345 -> {HashApiKey(key1)}");
Console.WriteLine($"prod-api-key-67890 -> {HashApiKey(key2)}");

static string HashApiKey(string apiKey)
{
    const int saltSize = 16;
    const int keySize = 32;
    const int iterations = 10000;
    
    byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
    using var pbkdf2 = new Rfc2898DeriveBytes(apiKey, salt, iterations, HashAlgorithmName.SHA256);
    byte[] hash = pbkdf2.GetBytes(keySize);
    
    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
}
```

Run with: `dotnet run`

## Pre-generated Hashed Keys

For the default API keys in this project:

### test-api-key-12345
```
Replace this with your generated hash
```

### prod-api-key-67890
```
Replace this with your generated hash
```

## How to Use Hashed Keys

1. Generate hashed versions of your API keys using one of the methods above
2. Update `appsettings.json` in InfoSecApp.Api:

```json
{
  "HashedApiKeys": [
    "base64salt:base64hash",
    "anothersalt:anotherhash"
  ]
}
```

3. Provide the **plain text** API key to clients (e.g., "test-api-key-12345")
4. Store only the **hashed version** in your configuration
5. The middleware will hash incoming keys and compare them securely

## Security Benefits

- **Secure Storage**: API keys are never stored in plain text
- **Salt Protection**: Each key has a unique random salt preventing rainbow table attacks
- **PBKDF2**: Uses 10,000 iterations of SHA-256 for strong key derivation
- **Timing Attack Protection**: Uses constant-time comparison to prevent timing attacks
- **Best Practice**: Follows OWASP recommendations for credential storage

## API Key Format

The hashed format is: `{salt}:{hash}`

- **Salt**: 16 bytes (128 bits) encoded as Base64
- **Hash**: 32 bytes (256 bits) PBKDF2-HMAC-SHA256 encoded as Base64
- **Iterations**: 10,000 rounds

Example:
```
gHJ3k9xKLm5pQr7sT8vW1A==:Kx2mN3pQ4rS5tU6vW7xY8zA9bC0dE1fG2hI3jK4lM5n=
```

## Verifying Keys

To verify if a key matches a hash, the ApiKeyHasher.VerifyApiKey method:
1. Extracts the salt from the stored hash
2. Hashes the provided key with that salt
3. Compares the computed hash with the stored hash using constant-time comparison
4. Returns true if they match

This ensures secure and timing-attack resistant verification.
