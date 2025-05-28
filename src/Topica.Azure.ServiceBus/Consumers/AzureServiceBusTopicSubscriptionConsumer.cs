using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Consumers;

public class AzureServiceBusTopicSubscriptionConsumer : IConsumer
{
    private readonly IServiceBusClientProvider _serviceBusClientProvider;
    private readonly IMessageHandlerExecutor _messageHandlerExecutor;
    private readonly MessagingSettings _messagingSettings;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ILogger _logger;

    public AzureServiceBusTopicSubscriptionConsumer(
        IServiceBusClientProvider serviceBusClientProvider,
        IMessageHandlerExecutor messageHandlerExecutor,
        MessagingSettings messagingSettings, 
        ILogger logger)
    {
        _serviceBusClientProvider = serviceBusClientProvider;
        _messageHandlerExecutor = messageHandlerExecutor;
        _messagingSettings = messagingSettings;
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

    public async Task ConsumeAsync<T>(CancellationToken cancellationToken) where T : IHandler
    {
        Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
        {
            _retryPipeline.ExecuteAsync(x => StartAsync<T>($"{typeof(T).Name}-consumer-({index})", _messagingSettings, x), cancellationToken);
        });
    }

    private async ValueTask StartAsync<T>(string consumerName, MessagingSettings messagingSettings, CancellationToken cancellationToken) where T : IHandler
    {
        try
        {
            _logger.LogInformation("{AwsQueueConsumerName}:: {ConsumerName} started on Queue: {Subscription}", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName, messagingSettings.SubscribeToSource);

            var opt = new ServiceBusReceiverOptions
            {
                Identifier = consumerName,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };
            
            var receiver = _serviceBusClientProvider.Client.CreateReceiver(messagingSettings.Source, messagingSettings.SubscribeToSource, opt);
                
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

            _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName, messagingSettings.SubscribeToSource);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName, messagingSettings.SubscribeToSource);
            await _serviceBusClientProvider.Client.DisposeAsync();
        }
        catch (AggregateException ex)
        {
            foreach (var inner in ex.Flatten().InnerExceptions)
            {
                _logger.LogError(inner, "{AwsQueueConsumerName}: {ConsumerName}: AggregateException:", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{AwsQueueConsumerName}: {ConsumerName}: Exception:", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName);
            throw;
        }
    }
}