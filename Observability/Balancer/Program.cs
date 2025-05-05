using Balancer;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;

services.AddHttpClient();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();
services.AddTransient<LoadBalancerMiddleware>();

var app = builder.Build();

app.UseMiddleware<LoadBalancerMiddleware>();

app.Run();
