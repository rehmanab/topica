using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqTopicCreationBuilder
    {
        IRabbitMqTopicCreationBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IRabbitMqTopicCreationBuilderWithTopicName
    {
        IRabbitMqTopicCreationBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IRabbitMqTopicCreationBuilderWithQueues
    {
        IRabbitMqTopicCreationBuilderWithQueueToSubscribeTo WithSubscribedQueues(string[] queueNames);
    }
    
    public interface IRabbitMqTopicCreationBuilderWithQueueToSubscribeTo
    {
        IRabbitMqTopicCreationBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IRabbitMqTopicCreationBuilderWithBuild
    {
        IRabbitMqTopicCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}