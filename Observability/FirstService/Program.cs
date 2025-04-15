using System.Net;
using Docker.DotNet;
using Microsoft.Net.Http.Headers;
using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

services.AddHttpClient("Balancer", httpClient => 
    httpClient.BaseAddress = new Uri(configuration["ApiGateway:Url"]!));
services.AddSingleton<IObservabilitySender>(new SenderHttpClient(new Uri(configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<ObservabilityMiddleware>();
app.MapGet("/hello", () => "Hello World!");

app.Run();