using Topica.Azure.ServiceBus.Settings;
using Topica.Contracts;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusConsumerTopicFluentBuilder
{
    IAzureServiceBusConsumerTopicBuilderWithTopic WithConsumerName(string consumerName);
}

public interface IAzureServiceBusConsumerTopicBuilderWithTopic
{
    IAzureServiceBusConsumerTopicBuilderWithSubscriptions WithTopicName(string topicName);
}
    
public interface IAzureServiceBusConsumerTopicBuilderWithSubscriptions
{
    IAzureServiceBusConsumerTopicBuilderWithTimings WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusConsumerTopicBuilderWithTimings
{
    IAzureServiceBusConsumerTopicBuilderWithOptions WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
}

public interface IAzureServiceBusConsumerTopicBuilderWithOptions
{
    IAzureServiceBusConsumerTopicBuilderWithMetadata WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
}

public interface IAzureServiceBusConsumerTopicBuilderWithMetadata
{
    IAzureServiceBusConsumerTopicBuilder WithMetadata(string? metadata);
}

public interface IAzureServiceBusConsumerTopicBuilder
{
    Task StartConsumingAsync<T>(string subscribeToSubscription, int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler;
}