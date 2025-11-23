using Topica.Contracts;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusQueueCreationBuilder
{
    IAzureServiceBusQueueCreationBuilderWithQueueName WithWorkerName(string workerName);
}

public interface IAzureServiceBusQueueCreationBuilderWithQueueName
{
    IAzureServiceBusQueueCreationBuilderWithBuild WithQueueName(string queueName);
}

public interface IAzureServiceBusQueueCreationBuilderWithBuild
{
    IAzureServiceBusQueueCreationBuilderWithBuild WithTimings(string? duplicateDetectionHistoryTimeWindow);
    IAzureServiceBusQueueCreationBuilderWithBuild WithOptions(bool? enableBatchedOperations, bool? enablePartitioning, int? maxSizeInMegabytes, int? maxMessageSizeInKilobytes, bool? enabledStatus, bool? supportOrdering);
    IAzureServiceBusQueueCreationBuilderWithBuild WithMetadata(string? metadata);
    IAzureServiceBusQueueCreationBuilderWithBuild WithNumberOfInstances(int? numberOfInstances);

    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}