using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Demo.RestApi.Services;

public class ServiceBusMessageService : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private const string TopicName = "demo-topic";

    public ServiceBusMessageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("SERVICE_BUS_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("SERVICE_BUS_CONNECTION_STRING not configured");
        }

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(TopicName);
    }

    public async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(jsonMessage)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender != null)
        {
            await _sender.DisposeAsync();
        }
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}