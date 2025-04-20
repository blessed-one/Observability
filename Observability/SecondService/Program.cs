using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IObservabilitySender>(
    new SenderHttpClient(new Uri(builder.Configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<ObservabilityMiddleware>();

app.MapGet("/hello2", () => "Hello from SECOND service!");

async Task<IResult> DoSomethingAsync()
{
    await Task.Delay(2500);

    var bug = Random.Shared.NextDouble();
    return bug < 0.95
        ? Results.Ok("__second")
        : Results.Problem("second service error");
}

app.MapGet("/DoSecond", DoSomethingAsync);

app.Run();