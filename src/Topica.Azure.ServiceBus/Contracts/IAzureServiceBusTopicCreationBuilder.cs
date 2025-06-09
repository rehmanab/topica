using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusTopicCreationBuilder
{
    IAzureServiceBusTopicBuilderWithTopicName WithWorkerName(string workerName);
}

public interface IAzureServiceBusTopicBuilderWithTopicName
{
    IAzureServiceBusTopicBuilderWithSubscriptions WithTopicName(string topicName);
}
    
public interface IAzureServiceBusTopicBuilderWithSubscriptions
{
    IAzureServiceBusTopicBuilderWithSubscribeToSubscription WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusTopicBuilderWithSubscribeToSubscription
{
    IAzureServiceBusTopicBuilder WithSubscribeToSubscription(string subscribeToSubscription);
}

public interface IAzureServiceBusTopicBuilder
{
    IAzureServiceBusTopicBuilder WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusTopicBuilder WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusTopicBuilder WithMetadata(string? metadata);
    IAzureServiceBusTopicBuilder WithNumberOfInstances(int? numberOfInstances);
    
    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}