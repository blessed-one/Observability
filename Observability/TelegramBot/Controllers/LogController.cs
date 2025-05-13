using Microsoft.AspNetCore.Mvc;
using TelegramBot.Abstractions;
using TelegramBot.Dto;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/logs")]
public class LogController(ITelegramBotService telegramBotService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendLog([FromBody] LogMessageDto logMessage)
    {
        try
        {
            if (logMessage.IsError)
                await telegramBotService.LogPublisher.LogError(logMessage.Message);
            else
                await telegramBotService.LogPublisher.Log(logMessage.Message);
            
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}