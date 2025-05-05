using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Realisation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Storage.Validators;
using Storage;
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

services.AddProblemDetails(); 
services.AddMongoDb(builder.Configuration["MongoDb:ConnectionString"]!);
services.AddValidation();


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

app.MapGet("/health", async (IMongoClient mongoClient) =>
{
    await mongoClient.ListDatabaseNamesAsync();
    return Results.Ok(new { status = "healthy", message = "MongoDB connection successful" });
});

app.MapGet("/records", async (MongoService mongoService) =>
{
    var records = await mongoService.GetAllRecordsAsync();
    return Results.Ok(records);
});

app.MapGet("/records/last/{n:int}", async (int n, MongoService mongoService) =>
{
    var records = await mongoService.GetLastNRecordsAsync(n);
    return Results.Ok(records);
});

app.MapPost("/add", async (
    [FromBody] ObservabilityRecord record,
    MongoService mongoService) =>
{
    var entity = await mongoService.AddRecordAsync(record);
    return Results.Ok(new { Message = "Record stored successfully", Id = entity.Id });
});

app.Run();