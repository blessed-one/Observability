using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Storage.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path;

            System.Console.WriteLine($"Request from {clientIp} to {path}");

            await _next(context);
        }
    }
} 