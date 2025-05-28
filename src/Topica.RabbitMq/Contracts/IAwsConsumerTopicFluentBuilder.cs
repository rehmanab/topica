using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqTopicFluentBuilder
    {
        IRabbitMqConsumerTopicBuilderWithTopic WithWorkerName(string workerName);
    }

    public interface IRabbitMqConsumerTopicBuilderWithTopic
    {
        IRabbitMqConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IRabbitMqConsumerTopicBuilderWithQueues
    {
        IRabbitMqConsumerTopicBuilder WithSubscribedQueues(string subscribeToQueueName, params string[] queueNames);
    }
    
    public interface IRabbitMqConsumerTopicBuilder
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}