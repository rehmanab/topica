using Topica.Contracts;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusQueueBuilder
{
    IAzureServiceBusQueueBuilderWithQueueName WithWorkerName(string workerName);
}

public interface IAzureServiceBusQueueBuilderWithQueueName
{
    IAzureServiceBusQueueBuilderWithBuild WithQueueName(string queueName);
}

public interface IAzureServiceBusQueueBuilderWithBuild
{
    IAzureServiceBusQueueBuilderWithBuild WithTimings(string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusQueueBuilderWithBuild WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusQueueBuilderWithBuild WithMetadata(string? metadata);
    IAzureServiceBusQueueBuilderWithBuild WithNumberOfInstances(int? numberOfInstances);
    
    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}