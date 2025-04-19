using Balancer;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddHttpClient();
services.AddSwaggerGen();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();
services.AddTransient<LoadBalancerMiddleware>();

var app = builder.Build();

app.UseMiddleware<LoadBalancerMiddleware>();

app.Run();
