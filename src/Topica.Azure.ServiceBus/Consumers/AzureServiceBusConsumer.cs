using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Settings;
using Topica.Contracts;
using Topica.Helpers;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Consumers;

public class AzureServiceBusConsumer : IConsumer
{
    private readonly IServiceBusClientProvider _serviceBusClientProvider;
    private readonly ITopicProviderFactory _topicProviderFactory;
    private readonly IMessageHandlerExecutor _messageHandlerExecutor;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ILogger<AzureServiceBusConsumer> _logger;

    public AzureServiceBusConsumer(
        IServiceBusClientProvider serviceBusClientProvider, 
        ITopicProviderFactory topicProviderFactory,
        IMessageHandlerExecutor messageHandlerExecutor,
        ILogger<AzureServiceBusConsumer> logger)
    {
        _serviceBusClientProvider = serviceBusClientProvider;
        _topicProviderFactory = topicProviderFactory;
        _messageHandlerExecutor = messageHandlerExecutor;
        _retryPipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.FromSeconds(5),
            MaxRetryAttempts = int.MaxValue,
            OnRetry = args =>
            {
                logger.LogWarning("Retrying: {ArgsAttemptNumber} in {RetryDelayTotalSeconds} seconds", args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                return default;
            }
        }).Build();
        _logger = logger;
    }

    public async Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
    {
        var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(_serviceBusClientProvider.ConnectionString);
        if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(".servicebus.windows.net"))
        {
            _logger.LogInformation("Azure Service Bus Consumer: {ConsumerName} with connection string endpoint: {ConnectionStringEndpoint}", consumerName, connectionStringEndpoint);
            await _topicProviderFactory.Create(MessagingPlatform.AzureServiceBus).CreateTopicAsync(consumerSettings);
        }
        else
        {
            _logger.LogInformation("Azure Service Bus Consumer: {ConsumerName} is using the Emulator endpoint: {ConnectionStringEndpoint} .. Skipping Creation as it's not supported", consumerName, connectionStringEndpoint);
        }

        Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
        {
            _retryPipeline.ExecuteAsync(x => StartAsync<T>($"{consumerName}-consumer-({index})", consumerSettings, x), cancellationToken);
        });
    }

    private async ValueTask StartAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
    {
        try
        {
            _logger.LogInformation("{AwsQueueConsumerName}:: {ConsumerName} started on Queue: {Subscription}", nameof(AzureServiceBusConsumer), consumerName, consumerSettings.SubscribeToSource);

            var opt = new ServiceBusReceiverOptions
            {
                Identifier = consumerName,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };
            
            var receiver = _serviceBusClientProvider.Client.CreateReceiver(consumerSettings.Source, consumerSettings.SubscribeToSource, opt);
                
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
                
                if (message == null) continue;
                
                var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync<T>(Encoding.UTF8.GetString(message.Body));
                // _logger.LogDebug("**** {AwsQueueConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} @ {DateTime} ****", nameof(AzureServiceBusConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                if (!success) continue;
                    
                await receiver.CompleteMessageAsync(message, cancellationToken);
                
                // await Task.Delay(1000, cancellationToken); // Simulate processing delay
            }

            _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AzureServiceBusConsumer), consumerName, consumerSettings.SubscribeToSource);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AzureServiceBusConsumer), consumerName, consumerSettings.SubscribeToSource);
            await _serviceBusClientProvider.Client.DisposeAsync();
        }
        catch (AggregateException ex)
        {
            foreach (var inner in ex.Flatten().InnerExceptions)
            {
                _logger.LogError(inner, "{AwsQueueConsumerName}: {ConsumerName}: AggregateException:", nameof(AzureServiceBusConsumer), consumerName);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{AwsQueueConsumerName}: {ConsumerName}: Exception:", nameof(AzureServiceBusConsumer), consumerName);
            throw;
        }
    }
}