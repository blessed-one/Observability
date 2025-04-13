using Balancer;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddHttpClient();
services.AddSwaggerGen();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();

var url = Environment.GetEnvironmentVariable("ApiGateway__Url");
Console.WriteLine(url);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LoadBalancerMiddleware>();

app.UseHttpsRedirection();

app.Run();
