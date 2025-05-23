using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Realisation;
using Storage.Services;
using Storage.Middleware;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Storage.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Configuration.AddEnvironmentVariables();

services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddHttpClient("Bot", client => 
{
    client.BaseAddress = new Uri("http://telegram-bot:8080");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

services.AddProblemDetails();
services.AddMongoDb(builder.Configuration["MongoDb:ConnectionString"]!);
services.AddValidation();
services.AddSingleton<WebSocketNotifier>();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = exception?.Message,
        });
    });
});

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<StorageKeyMiddleware>();
app.UseWebSockets();

app.MapGet("/health", async (IMongoClient mongoClient) =>
{
    await mongoClient.ListDatabaseNamesAsync();
    return Results.Ok(new { status = "healthy", message = "MongoDB connection successful" });
});

app.MapGet("/records", async (MongoService mongoService) =>
{
    var records = (await mongoService.GetAllRecordsAsync())
        .Select(r => new
        {
            r.TraceId,
            r.ParentId,
            r.NodeId,
            Method = r.HttpRequestData["method"],
            Path = r.HttpRequestData["path"],
            r.IsError,
            r.Timestamp,
            r.Message
        });

    return Results.Ok(records);
});

app.MapGet("/records-debug", async (MongoService mongoService) =>
{
    var records = await mongoService.GetAllRecordsAsync();

    return Results.Ok(records);
});

app.MapGet("/metrics", async (MongoService mongoService) =>
{
    var metrics = (await mongoService.GetAllRecordsAsync())
        .Select(r => new
        {
            r.NodeId,
            r.TraceId,
            r.Timestamp,
            Cpu = r.MetricData!["cpu"],
            Duration = r.MetricData!["duration"],
            Memory = r.MetricData!["memory"]
        });

    return Results.Ok(metrics);
});

app.MapGet("/records/last/{n:int}", async (int n, MongoService mongoService) =>
{
    var records = await mongoService.GetLastNRecordsAsync(n);
    return Results.Ok(records);
});

app.MapPost("/add", async (
    [FromBody] ObservabilityRecord record,
    MongoService mongoService,
    HttpContext context,
    WebSocketNotifier notifier) =>
{
    var entity = await mongoService.AddRecordAsync(record);
    
    var httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>()
        .CreateClient("Bot");
    
    var logMessage = new 
    {
        Message = record.Message ?? "всё хорошо",
        IsError = record.IsError
    };
    
    var response = await httpClient.PostAsJsonAsync("api/logs/send", logMessage);
    
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error from bot API: {response.StatusCode}, {errorContent}");
    }
    
    await notifier.NotifyAllAsync(record);
  
    return Results.Ok(new { Message = "Record stored successfully", Id = entity.Id });
});

app.MapGet("/records/trace/{traceId}", async (string traceId, MongoService mongoService) =>
{
    var records = await mongoService.GetRecordsByTraceIdAsync(traceId);
    return Results.Ok(records);
});

app.MapGet("/records/range", async (
    [FromQuery] DateTime start,
    [FromQuery] DateTime end,
    MongoService mongoService) =>
{
    var records = await mongoService.GetRecordsByTimeRangeAsync(start, end);
    return Results.Ok(records);
});

app.MapGet("/records/host/{host}", async (string host, MongoService mongoService) =>
{
    var records = await mongoService.GetRecordsByHostAsync(host);
    return Results.Ok(records);
});

app.Map("/ws/updates", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        var notifier = context.RequestServices.GetRequiredService<WebSocketNotifier>();
        notifier.AddClient(ws);
        var buffer = new byte[1024 * 4];
        while (ws.State == System.Net.WebSockets.WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                break;
        }
        notifier.RemoveClient(ws);
        await ws.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();