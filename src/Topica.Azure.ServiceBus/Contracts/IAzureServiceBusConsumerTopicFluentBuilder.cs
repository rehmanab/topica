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
    IAzureServiceBusConsumerTopicBuilder WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusConsumerTopicBuilder
{
    IAzureServiceBusConsumerTopicBuilder WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusConsumerTopicBuilder WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusConsumerTopicBuilder WithMetadata(string? metadata);
    Task StartConsumingAsync<T>(string subscribeToSubscription, int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler;
}