using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Realisation.Abstractions;

namespace Realisation;

public class ObservabilityMiddleware(IObservabilitySender sender, RequestDelegate next, string nodeId)
{
    private static readonly ActivitySource ActivitySource = new("Observability");
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = new ObservabilityActivity(
            ActivitySource.StartActivity($"{context.Request.Path}")!, nodeId, context);

        try
        {
            activity.Start();

            await next(context);
        }
        catch (Exception e)
        {
            activity.Error(e.Message);
        }
        finally
        {
            activity.Stop();
            await sender.SendAsync(activity.GetActivityJson());
        }
    }
}