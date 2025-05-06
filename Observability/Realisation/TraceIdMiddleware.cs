using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Realisation
{
    public class TraceIdMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Add("trace-parent-id", context.TraceIdentifier);
            await next(context);
        }
    }
}