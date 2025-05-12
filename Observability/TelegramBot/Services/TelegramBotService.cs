using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBot.Abstractions;

namespace TelegramBot.Services;

public class TelegramBotService(IConfiguration config, ILogEventPublisher logPublisher) 
    : ITelegramBotService
{
    public ILogEventPublisher LogPublisher { get; } = logPublisher;
    private readonly ITelegramBotClient _botClient = new TelegramBotClient(config["Telegram_Bot_Token"]!);
    private readonly Dictionary<long, bool> _userSettings = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogPublisher.OnLogMessage += HandleLogMessage;
        LogPublisher.OnErrorLogMessage += HandleErrorLogMessage;

        var receiverOptions = new ReceiverOptions();
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleLogMessage(string message)
    {
        await SendLogAsync(message, isError: false);
    }

    private async Task HandleErrorLogMessage(string errorMessage)
    {
        await SendLogAsync(errorMessage, isError: true);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
        CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        
        var chatId = message.Chat.Id;
        
        if (message.Text == "/start")
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Выберите режим логов:\n1. Все логи (/all)\n2. Только ошибки (/errors)",
                cancellationToken: cancellationToken);
        }
        else if (message.Text == "/all")
        {
            _userSettings[chatId] = false;
            await botClient.SendMessage(
                chatId: chatId,
                text: "Режим: Все логи",
                cancellationToken: cancellationToken);
        }
        else if (message.Text == "/errors")
        {
            _userSettings[chatId] = true;
            await botClient.SendMessage(
                chatId: chatId,
                text: "Режим: Только ошибки",
                cancellationToken: cancellationToken);
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
    }

    public async Task SendLogAsync(string message, bool isError)
    {
        foreach (var (chatId, sendOnlyErrors) in _userSettings)
        {
            if (!sendOnlyErrors || (sendOnlyErrors && isError))
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: isError ? $"Ошибка: {message}" : $"Лог: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to {chatId}: {ex.Message}");
                }
            }
        }
    }
}