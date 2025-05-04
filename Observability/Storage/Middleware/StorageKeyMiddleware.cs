using System.Net;

namespace Storage.Middleware;

public class StorageKeyMiddleware(RequestDelegate next)
{
    private const string StorageKey = "avava228";
    private const string StorageKeyHeader = "Storage-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(StorageKeyHeader, out var headerValue) || 
            headerValue != StorageKey)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                status = "unauthorized",
                message = "Invalid or missing Storage-Key header"
            });
            return;
        }

        await next(context);
    }
} 