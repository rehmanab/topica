using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqConsumerTopicFluentBuilder
    {
        IRabbitMqConsumerTopicBuilderWithTopic WithConsumerName(string consumerName);
    }

    public interface IRabbitMqConsumerTopicBuilderWithTopic
    {
        IRabbitMqConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IRabbitMqConsumerTopicBuilderWithQueues
    {
        IRabbitMqConsumerTopicBuilder WithSubscribedQueues(params string[] queueNames);
    }
    
    public interface IRabbitMqConsumerTopicBuilder
    {
        Task StartConsumingAsync<T>(string subscribeToQueueName, int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler;
    }
}