using Balancer;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddHttpClient();
services.AddSwaggerGen();
services.AddSingleton<ILoadBalancer, RoundRobinBalancer>();

var app = builder.Build();

app.UseMiddleware<LoadBalancerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
