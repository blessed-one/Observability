using TelegramBot.Abstractions;

namespace TelegramBot.Services;

public class LogEventPublisher : ILogEventPublisher
{
    public event Func<string, Task>? OnLogMessage;
    
    public event Func<string, Task>? OnErrorLogMessage;

    public async Task Log(string message)
    {
        OnLogMessage?.Invoke(message);
    }

    public async Task LogError(string errorMessage)
    {
        OnErrorLogMessage?.Invoke(errorMessage);
    }
}