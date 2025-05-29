using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Demo.MessageListener;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private ServiceBusClient? _serviceBusClient;
    private ServiceBusProcessor? _processor;
    private const string TopicName = "demo-topic";
    private const string SubscriptionName = "demo-subscription";

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting message listener service...");

        var connectionString = _configuration.GetValue<string>("SERVICE_BUS_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("SERVICE_BUS_CONNECTION_STRING not configured");
        }

        // Create a Service Bus client and processor for the topic subscription
        _serviceBusClient = new ServiceBusClient(connectionString);
        _processor = _serviceBusClient.CreateProcessor(TopicName, SubscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        // Add handler for processing messages
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        // Start processing
        await _processor.StartProcessingAsync(cancellationToken);
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Message listener running at: {time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping message listener service...");

        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received message: {body}", body);

            // You can deserialize and process the message here if needed
            // var message = JsonSerializer.Deserialize<TestMessage>(body);

            // Complete the message
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            await args.DeadLetterMessageAsync(args.Message, "Processing failed", ex.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error handling message");
        return Task.CompletedTask;
    }
}
