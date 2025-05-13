namespace TelegramBot.Abstractions;

public interface ILogEventPublisher
{
    public event Func<string, Task>? OnLogMessage;
    
    public event Func<string, Task>? OnErrorLogMessage;
    
    Task Log(string message);
    Task LogError(string errorMessage);
}