using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpClient("Balancer", httpClient =>
    httpClient.BaseAddress = new Uri(builder.Configuration["ApiGateway:Url"]!));
builder.Services.AddSingleton<IObservabilitySender>(
    new SenderHttpClient(new Uri(builder.Configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<ObservabilityMiddleware>();

app.MapGet("/hello1", () => "Hello from FIRST service!");

app.MapGet("/DoFirst", async (HttpContext context) =>
{
    await Task.Delay(5000);

    try
    {
        var client = context.RequestServices.GetRequiredService<HttpClient>();

        var traceId = context.Items["TraceId"]?.ToString()
                      ?? throw new InvalidOperationException("Trace ID not found");

        client.DefaultRequestHeaders.Add("traceparent", traceId);

        var response = await client.GetAsync("/secondService");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var modifiedContent = $"FIR__{content}";

        await Task.Delay(5000);

        return Results.Ok(modifiedContent);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Service error",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});

app.Run();