using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Text.Json;

namespace Demo.RestApi.Services;

public class ServiceBusMessageService : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusAdministrationClient _adminClient;
    private const string TopicName = "demo-topic";
    private static readonly string[] SubscriptionNames = ["demo-subscription"]; // Add all required subscription names here

    public ServiceBusMessageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("SERVICE_BUS_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("SERVICE_BUS_CONNECTION_STRING not configured");
        }


        _adminClient = new ServiceBusAdministrationClient(connectionString);
        _client = new ServiceBusClient(connectionString);

        EnsureTopicsAndSubscriptionsAsync().GetAwaiter().GetResult();
        _sender = _client.CreateSender(TopicName);
    }

    public async Task SendMessageAsync<T>(T message, string? subject = null, CancellationToken cancellationToken = default)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(jsonMessage)
        {
            ContentType = "application/json",
            Subject = subject ?? typeof(T).Name
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    private async Task EnsureTopicsAndSubscriptionsAsync()
    {
        // Ensure topic exists
        if (!await _adminClient.TopicExistsAsync(TopicName))
        {
            await _adminClient.CreateTopicAsync(TopicName);
        }

        // Ensure all required subscriptions exist
        foreach (var subscriptionName in SubscriptionNames)
        {
            if (!await _adminClient.SubscriptionExistsAsync(TopicName, subscriptionName))
            {
                await _adminClient.CreateSubscriptionAsync(TopicName, subscriptionName);
            }
        }
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