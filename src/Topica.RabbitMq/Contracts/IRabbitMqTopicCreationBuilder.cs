using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqTopicCreationBuilder
    {
        IRabbitMqTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IRabbitMqTopicBuilderWithTopicName
    {
        IRabbitMqTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IRabbitMqTopicBuilderWithQueues
    {
        IRabbitMqTopicBuilderWithQueueToSubscribeTo WithSubscribedQueues(params string[] queueNames);
    }
    
    public interface IRabbitMqTopicBuilderWithQueueToSubscribeTo
    {
        IRabbitMqTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IRabbitMqTopicBuilderWithBuild
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}