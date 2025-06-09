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
    private readonly IAzureServiceBusClientProvider _azureServiceBusClientProvider;
    private readonly IMessageHandlerExecutor _messageHandlerExecutor;
    private readonly MessagingSettings _messagingSettings;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ILogger _logger;

    public AzureServiceBusTopicSubscriptionConsumer(
        IAzureServiceBusClientProvider azureServiceBusClientProvider,
        IMessageHandlerExecutor messageHandlerExecutor,
        MessagingSettings messagingSettings, 
        ILogger logger)
    {
        _azureServiceBusClientProvider = azureServiceBusClientProvider;
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

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
        {
            _retryPipeline.ExecuteAsync(x => StartAsync($"{_messagingSettings.WorkerName}-({index})", _messagingSettings, x), cancellationToken);
        });
    }

    private async ValueTask StartAsync(string consumerName, MessagingSettings messagingSettings, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("**** CONSUMER STARTED: {ConsumerName} started on Queue: {Subscription}", consumerName, messagingSettings.SubscribeToSource);

            var opt = new ServiceBusReceiverOptions
            {
                Identifier = consumerName,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };
            
            var receiver = _azureServiceBusClientProvider.Client.CreateReceiver(messagingSettings.Source, messagingSettings.SubscribeToSource, opt);
                
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
                
                if (message == null) continue;
                
                var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(Encoding.UTF8.GetString(message.Body));
                // _logger.LogDebug("**** {Name}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(AzureServiceBusTopicSubscriptionConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");

                if (!success) continue;
                    
                await receiver.CompleteMessageAsync(message, cancellationToken);
                
                // await Task.Delay(1000, cancellationToken); // Simulate processing delay
            }

            _logger.LogInformation("**** CONSUMER STOPPED: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", consumerName, messagingSettings.SubscribeToSource);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("**** ERROR: CONSUMER STOPPED: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", consumerName, messagingSettings.SubscribeToSource);
            await _azureServiceBusClientProvider.Client.DisposeAsync();
        }
        catch (AggregateException ex)
        {
            foreach (var inner in ex.Flatten().InnerExceptions)
            {
                _logger.LogError(inner, "****ERROR: CONSUMER STOPPED: {ConsumerName}: AggregateException:", consumerName);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "**** CONSUMER STOPPED: {ConsumerName}: Exception:", consumerName);
            throw;
        }
    }
}