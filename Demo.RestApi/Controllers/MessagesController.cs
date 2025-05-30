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
    public async Task<IActionResult> SendTestMessage([FromBody] TestMessage message, [FromQuery] string? subject = null)
    {
        try
        {
            // Allow subject override via body (optional)
            var effectiveSubject = subject;
            if (string.IsNullOrWhiteSpace(effectiveSubject) && !string.IsNullOrWhiteSpace(message.Text))
            {
                // If the message text starts with 'subject:', use that as subject
                if (message.Text.StartsWith("subject:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = message.Text.Split(':', 2);
                    if (parts.Length == 2)
                        effectiveSubject = parts[1].Trim();
                }
            }
            await _messagingService.SendMessageAsync(message, effectiveSubject);
            _logger.LogInformation("Test message sent successfully: {text} | Subject: {subject}", message.Text, effectiveSubject);
            return Ok(new { message = "Message sent successfully", subject = effectiveSubject });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test message");
            return StatusCode(500, new { error = "Failed to send message", details = ex.Message });
        }
    }
}