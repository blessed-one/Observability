using Microsoft.OpenApi.Models;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Observability API", Version = "v1", Description = "API для визуализации трейсов" });
});

// Добавляем HttpClient для Storage
builder.Services.AddHttpClient("Storage", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Storage:Url"]!);
    client.DefaultRequestHeaders.Add("Storage-Key", "avava228");
});

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Observability API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();


app.MapGet("/api/traces/debug", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("Storage");
    var response = await client.GetFromJsonAsync<object[]>("/records-debug");
    return Results.Ok(response);
})
.WithName("GetTracesDebug")
.WithDescription("Получить все трейсы с полными данными");

app.MapFallbackToFile("index.html");

app.Run();