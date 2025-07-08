using Topica.Contracts;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusTopicBuilder
{
    IAzureServiceBusTopicBuilderWithTopicName WithWorkerName(string workerName);
}

public interface IAzureServiceBusTopicBuilderWithTopicName
{
    IAzureServiceBusTopicBuilderWithSubscribeToSubscription WithTopicName(string topicName);
}

public interface IAzureServiceBusTopicBuilderWithSubscribeToSubscription
{
    IAzureServiceBusTopicBuilderWithBuild WithSubscribeToSubscription(string subscribeToSubscription);
}

public interface IAzureServiceBusTopicBuilderWithBuild
{
    IAzureServiceBusTopicBuilderWithBuild WithNumberOfInstances(int? numberOfInstances);
    
    Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
    Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
}