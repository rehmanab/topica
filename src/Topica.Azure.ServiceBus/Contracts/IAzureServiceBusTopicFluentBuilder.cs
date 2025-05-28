using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusTopicFluentBuilder
{
    IAzureServiceBusTopicBuilderWithTopicName WithWorkerName(string workerName);
}

public interface IAzureServiceBusTopicBuilderWithTopicName
{
    IAzureServiceBusTopicBuilderWithSubscriptions WithTopicName(string topicName);
}
    
public interface IAzureServiceBusTopicBuilderWithSubscriptions
{
    IAzureServiceBusTopicBuilder WithSubscriptions(params AzureServiceBusTopicSubscriptionSettings[] subscriptions);
}

public interface IAzureServiceBusTopicBuilder
{
    IAzureServiceBusTopicBuilder WithTimings(string? autoDeleteOnIdle, string? defaultMessageTimeToLive, string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusTopicBuilder WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, bool? requiresDuplicateDetection, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusTopicBuilder WithMetadata(string? metadata);
    Task<IConsumer> BuildConsumerAsync(string subscribeToSubscription, int? numberOfInstances, CancellationToken cancellationToken = default);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}