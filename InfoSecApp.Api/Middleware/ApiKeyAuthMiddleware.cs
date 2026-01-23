using InfoSecApp.Api.Services;

namespace InfoSecApp.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for health check and OpenAPI endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
        var hashedApiKeys = appSettings.GetSection("HashedApiKeys").Get<List<string>>() ?? new List<string>();

        // Verify the provided API key against all stored hashed keys
        bool isValid = false;
        foreach (var hashedKey in hashedApiKeys)
        {
            if (ApiKeyHasher.VerifyApiKey(extractedApiKey!, hashedKey))
            {
                isValid = true;
                break;
            }
        }

        if (!isValid)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
