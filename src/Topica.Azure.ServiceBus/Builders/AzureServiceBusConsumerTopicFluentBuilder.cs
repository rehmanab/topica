using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Builders;

public class AzureServiceBusConsumerTopicFluentBuilder(IConsumer consumer) : IAzureServiceBusConsumerTopicFluentBuilder, IAzureServiceBusConsumerTopicBuilderWithTopic, IAzureServiceBusConsumerTopicBuilderWithSubscriptions, IAzureServiceBusConsumerTopicBuilderWithTimings, IAzureServiceBusConsumerTopicBuilderWithOptions, IAzureServiceBusConsumerTopicBuilderWithMetadata, IAzureServiceBusConsumerTopicBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private Settings.AzureServiceBusTopicSubscriptionSettings[] _subscriptions = null!;
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

    public IAzureServiceBusConsumerTopicBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IAzureServiceBusConsumerTopicBuilderWithSubscriptions WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IAzureServiceBusConsumerTopicBuilderWithTimings WithSubscriptions(params Settings.AzureServiceBusTopicSubscriptionSettings[] subscriptions)
    {
        _subscriptions = subscriptions;
        return this;
    }

    public IAzureServiceBusConsumerTopicBuilderWithOptions WithTimings(
        string? autoDeleteOnIdle,
        string? defaultMessageTimeToLive, 
        string? duplicateDetectionHistoryTimeWindow)
    {
        _autoDeleteOnIdle = autoDeleteOnIdle;
        _defaultMessageTimeToLive = defaultMessageTimeToLive;
        _duplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
        return this;
    }

    public IAzureServiceBusConsumerTopicBuilderWithMetadata WithOptions(
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

    public IAzureServiceBusConsumerTopicBuilder WithMetadata(string? metadata)
    {
        _metadata = metadata;
        return this;
    }

    public async Task StartConsumingAsync<T>(string subscribeToSubscription, int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler
    {
        var instances = numberOfInstances switch
        {
            < 1 => 1,
            > 10 => 10,
            _ => numberOfInstances
        };

        var consumerSettings = new ConsumerSettings
        {
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
            SubscribeToSource = subscribeToSubscription,
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
            NumberOfInstances = instances
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }
}