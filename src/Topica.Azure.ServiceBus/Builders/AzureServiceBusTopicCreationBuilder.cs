using Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Helpers;
using Topica.Infrastructure.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Builders;

public class AzureServiceBusTopicCreationBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    IAzureServiceBusClientProvider provider, 
    ILogger<AzureServiceBusTopicCreationBuilder> logger) 
    : IAzureServiceBusTopicCreationBuilder, IAzureServiceBusTopicBuilderWithTopicName, IAzureServiceBusTopicBuilderWithSubscriptions, IAzureServiceBusTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private AzureServiceBusTopicSubscriptionSettings[] _subscriptions = null!;
    private string? _autoDeleteOnIdle;
    private string? _defaultMessageTimeToLive;
    private string? _duplicateDetectionHistoryTimeWindow;
    private bool? _enableBatchedOperations = true;
    private bool? _enablePartitioning = false;
    private int? _maxSizeInMegabytes = 1024;
    private bool? _requiresDuplicateDetection = true;
    private int? _maxMessageSizeInKilobytes = 256;
    private bool? _enabledStatus = true;
    private bool? _supportOrdering = false;
    private string? _metadata;

    public IAzureServiceBusTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithSubscriptions WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions)
    {
        _subscriptions = subscriptions;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithTimings(
        string? autoDeleteOnIdle,
        string? defaultMessageTimeToLive, 
        string? duplicateDetectionHistoryTimeWindow)
    {
        _autoDeleteOnIdle = autoDeleteOnIdle;
        _defaultMessageTimeToLive = defaultMessageTimeToLive;
        _duplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithOptions(
        bool? enableBatchedOperations, 
        bool? enablePartitioning,
        int? maxSizeInMegabytes, 
        bool? requiresDuplicateDetection, 
        int? maxMessageSizeInKilobytes, 
        bool? enabledStatus,
        bool? supportOrdering)
    {
        _enableBatchedOperations = enableBatchedOperations;
        _enablePartitioning = enablePartitioning;
        _maxSizeInMegabytes = maxSizeInMegabytes;
        _requiresDuplicateDetection = requiresDuplicateDetection;
        _maxMessageSizeInKilobytes = maxMessageSizeInKilobytes;
        _enabledStatus = enabledStatus;
        _supportOrdering = supportOrdering;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithMetadata(string? metadata)
    {
        _metadata = metadata;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(string subscribeToSubscription, int? numberOfInstances, CancellationToken cancellationToken = default)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.AzureServiceBus);
        var messagingSettings = GetMessagingSettings(subscribeToSubscription, numberOfInstances);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.AzureServiceBus, _workerName, messagingSettings.Source);
        
        var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(provider.ConnectionString);
        if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(".servicebus.windows.net"))
        {
            logger.LogInformation("Azure Service Bus Consumer: {ConsumerName} with connection string endpoint: {ConnectionStringEndpoint}", _workerName, connectionStringEndpoint);
            await pollyRetryService.WaitAndRetryAsync<ServiceBusException>
            (
                30,
                _ => TimeSpan.FromSeconds(10),
                (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(AzureServiceBusTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating topic, queue, subscriptions."),
                () => topicProvider.CreateTopicAsync(messagingSettings),
                false
            );
        }
        else
        {
            logger.LogInformation("Azure Service Bus Consumer: {ConsumerName} is using the Emulator endpoint: {ConnectionStringEndpoint} .. Skipping Creation as it's not supported", _workerName, connectionStringEndpoint);
        }

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();

        var topicProvider = topicProviderFactory.Create(MessagingPlatform.AzureServiceBus);
            
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.AzureServiceBus, _workerName, messagingSettings.Source);
        
        var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(provider.ConnectionString);
        if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(".servicebus.windows.net"))
        {
            logger.LogInformation("Azure Service Bus Producer: {ProducerName} with connection string endpoint: {ConnectionStringEndpoint}", _workerName, connectionStringEndpoint);
            await pollyRetryService.WaitAndRetryAsync<ServiceBusException>
            (
                30,
                _ => TimeSpan.FromSeconds(10),
                (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(AzureServiceBusTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating topic, queue, subscriptions."),
                () => topicProvider.CreateTopicAsync(messagingSettings),
                false
            );
        }
        else
        {
            logger.LogInformation("Azure Service Bus Producer: {ProducerName} is using the Emulator endpoint: {ConnectionStringEndpoint} .. Skipping Creation as it's not supported", _workerName, connectionStringEndpoint);
        }

        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(string? subscribeToSubscription = null, int? numberOfInstances = null)
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            AzureServiceBusSubscriptions = _subscriptions.Select(x => new AzureServiceBusTopicSubscriptionSettings
            {
                AutoDeleteOnIdle = x.AutoDeleteOnIdle,
                DefaultMessageTimeToLive = x.DefaultMessageTimeToLive,
                MaxDeliveryCount = x.MaxDeliveryCount,
                DeadLetteringOnMessageExpiration = x.DeadLetteringOnMessageExpiration,
                EnableBatchedOperations = x.EnableBatchedOperations,
                EnableDeadLetteringOnFilterEvaluationExceptions = x.EnableDeadLetteringOnFilterEvaluationExceptions,
                ForwardDeadLetteredMessagesTo = x.ForwardDeadLetteredMessagesTo,
                ForwardTo = x.ForwardTo,
                LockDuration = x.LockDuration,
                RequiresSession = x.RequiresSession,
                EnabledStatus = x.EnabledStatus,
                UserMetadata = x.UserMetadata,
                Source = x.Source
            }).ToArray(),
            SubscribeToSource = subscribeToSubscription ?? string.Empty,
            AzureServiceBusAutoDeleteOnIdle = _autoDeleteOnIdle,
            AzureServiceBusDefaultMessageTimeToLive = _defaultMessageTimeToLive,
            AzureServiceBusDuplicateDetectionHistoryTimeWindow = _duplicateDetectionHistoryTimeWindow,
            AzureServiceBusEnableBatchedOperations = _enableBatchedOperations,
            AzureServiceBusEnablePartitioning = _enablePartitioning,
            AzureServiceBusMaxSizeInMegabytes = _maxSizeInMegabytes,
            AzureServiceBusRequiresDuplicateDetection = _requiresDuplicateDetection,
            AzureServiceBusMaxMessageSizeInKilobytes = _maxMessageSizeInKilobytes,
            AzureServiceBusEnabledStatus = _enabledStatus,
            AzureServiceBusSupportOrdering = _supportOrdering,
            AzureServiceBusUserMetadata = _metadata,
            NumberOfInstances = numberOfInstances ?? 1
        };
    }
}