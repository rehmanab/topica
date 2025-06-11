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
        IRabbitMqTopicBuilderWithQueueToSubscribeTo WithSubscribedQueues(string[] queueNames);
    }
    
    public interface IRabbitMqTopicBuilderWithQueueToSubscribeTo
    {
        IRabbitMqTopicBuilder WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IRabbitMqTopicBuilder
    {
        IRabbitMqTopicBuilder WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}