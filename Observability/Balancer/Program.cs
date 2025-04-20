using Balancer;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var services = builder.Services;

services.AddHttpClient();
services.AddSwaggerGen();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();
services.AddTransient<LoadBalancerMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<LoadBalancerMiddleware>();

app.Run();
