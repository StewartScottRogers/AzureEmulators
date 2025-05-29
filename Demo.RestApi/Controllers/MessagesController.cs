using Microsoft.AspNetCore.Mvc;
using Demo.RestApi.Models;
using Demo.RestApi.Services;

namespace Demo.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ServiceBusMessageService _messagingService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(ServiceBusMessageService messagingService, ILogger<MessagesController> logger)
    {
        _messagingService = messagingService;
        _logger = logger;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTestMessage([FromBody] TestMessage message)
    {
        try
        {
            await _messagingService.SendMessageAsync(message);
            _logger.LogInformation("Test message sent successfully: {text}", message.Text);
            return Ok(new { message = "Message sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test message");
            return StatusCode(500, new { error = "Failed to send message", details = ex.Message });
        }
    }
}