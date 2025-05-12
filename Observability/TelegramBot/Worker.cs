using TelegramBot.Abstractions;

namespace TelegramBot
{
    public class Worker(ITelegramBotService telegramBotService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await telegramBotService.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(50, stoppingToken);
            }
        }
    }
}