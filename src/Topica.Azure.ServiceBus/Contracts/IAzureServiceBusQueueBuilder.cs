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
    IAzureServiceBusQueueBuilderWithBuild WithNumberOfInstances(int? numberOfInstances);
    
    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}