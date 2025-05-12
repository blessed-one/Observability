using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Abstractions;

public interface ITelegramBotService
{
    ILogEventPublisher LogPublisher { get; }

    Task StartAsync(CancellationToken cancellationToken);
    
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
        CancellationToken cancellationToken = default);

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken = default);

    Task SendLogAsync(string message, bool isError);
}