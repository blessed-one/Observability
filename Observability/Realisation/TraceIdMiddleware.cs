using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Realisation
{
    public class TraceIdMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (Activity.Current != null)
            {
                context.Items["TraceId"] = Activity.Current.TraceId.ToString();
            }

            await next(context);
        }
    }
}