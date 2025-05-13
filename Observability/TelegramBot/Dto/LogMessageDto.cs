namespace TelegramBot.Dto
{
    public record LogMessageDto(
        string Message, 
        bool IsError = false
        );
}