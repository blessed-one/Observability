using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpClient("Balancer", httpClient =>
    httpClient.BaseAddress = new Uri("http://localhost:5049"));
builder.Services.AddSingleton<IObservabilitySender>(
    new SenderHttpClient(new Uri(builder.Configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<ObservabilityMiddleware>("fir");

app.MapGet("/hello1", () => "Hello from FIRST service!");

app.MapGet("/DoFirst", async (HttpContext context) =>
{
    await Task.Delay(1000);

    try
    {
        var client = context.RequestServices.GetRequiredService<HttpClient>();

        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            Headers = { {"trace-parent-id", context.TraceIdentifier }},
            RequestUri = new Uri("http://balancer:8080/DoSecond")
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var modifiedContent = $"FIR__{content}";

        await Task.Delay(1000);

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