using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

services.AddHttpClient("Balancer", httpClient =>
{
    httpClient.BaseAddress = new Uri(configuration["ApiGateway:Url"]);
});
services.AddHttpClient("Storage", httpClient =>
{
    httpClient.BaseAddress = new Uri(configuration["ApiGateway:Url"]);
});

var app = builder.Build();

app.MapGet("/hello", () =>
{
    Console.WriteLine(1);
    var gatewayUrl = configuration.GetSection("ApiGateway:Url").Value;
    return gatewayUrl;
});

app.Run();
