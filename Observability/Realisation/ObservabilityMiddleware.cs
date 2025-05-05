using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Realisation.Abstractions;

namespace Realisation;

public class ObservabilityMiddleware(IObservabilitySender sender, RequestDelegate next, string prefix)
{
    private readonly string nodeId = $"{prefix}-{Environment.GetEnvironmentVariable("HOSTNAME")}";
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = new ObservabilityActivity(nodeId, context);

        try
        {
            activity.Start();

            await next(context);
        }
        catch (Exception e)
        {
            activity.Error(e.Message);
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(e.Message);
        }
        finally
        {
            activity.Stop();
            await sender.SendAsync(activity.GetActivityJson());
        }
    }
}