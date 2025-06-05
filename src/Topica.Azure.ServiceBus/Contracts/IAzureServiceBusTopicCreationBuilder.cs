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
    IAzureServiceBusTopicBuilderWithBuild WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusTopicBuilderWithBuild
{
    IAzureServiceBusTopicBuilderWithBuild WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusTopicBuilderWithBuild WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusTopicBuilderWithBuild WithMetadata(string? metadata);
    Task<IConsumer> BuildConsumerAsync(string subscribeToSubscription, int? numberOfInstances, CancellationToken cancellationToken = default);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}