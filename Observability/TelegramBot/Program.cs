using TelegramBot;
using TelegramBot.Abstractions;
using TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<ILogEventPublisher, LogEventPublisher>();
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();