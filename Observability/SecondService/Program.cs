using Realisation;
using Realisation.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IObservabilitySender>(
    new SenderHttpClient(new Uri(builder.Configuration["Storage:Url"]!)));

var app = builder.Build();

app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<ObservabilityMiddleware>("sec");

app.MapGet("/hello2", () => "Hello from SECOND service!");

async Task<IResult> DoSomethingAsync()
{
    await Task.Delay(2500);

    var bug = Random.Shared.NextDouble();
    
    if (bug > 1.95)
        throw new Exception("something went wrong");

    return Results.Ok("__second");
}

app.MapGet("/DoSecond", DoSomethingAsync);

app.Run();