namespace Demo.RestApi.Models;

public class TestMessage
{
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}