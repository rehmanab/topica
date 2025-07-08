using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusTopicCreationBuilder
{
    IAzureServiceBusTopicCreationBuilderWithTopicName WithWorkerName(string workerName);
}

public interface IAzureServiceBusTopicCreationBuilderWithTopicName
{
    IAzureServiceBusTopicCreationBuilderWithSubscriptions WithTopicName(string topicName);
}
    
public interface IAzureServiceBusTopicCreationBuilderWithSubscriptions
{
    IAzureServiceBusTopicCreationBuilderWithSubscribeToSubscription WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusTopicCreationBuilderWithSubscribeToSubscription
{
    IAzureServiceBusTopicCreationBuilderWithBuild WithSubscribeToSubscription(string subscribeToSubscription);
}

public interface IAzureServiceBusTopicCreationBuilderWithBuild
{
    IAzureServiceBusTopicCreationBuilderWithBuild WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusTopicCreationBuilderWithBuild WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusTopicCreationBuilderWithBuild WithMetadata(string? metadata);
    IAzureServiceBusTopicCreationBuilderWithBuild WithNumberOfInstances(int? numberOfInstances);
    
    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}