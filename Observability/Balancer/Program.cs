using Balancer;
using DotNetEnv;
using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;

services.AddHttpClient();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();
services.AddSingleton<LoadBalancerMiddleware>();
services.AddSingleton<IObservabilitySender>(
    new SenderHttpClient(new Uri(builder.Configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<ObservabilityMiddleware>("balance");
app.UseMiddleware<LoadBalancerMiddleware>();

app.Run();
