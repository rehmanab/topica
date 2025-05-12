using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsConsumerTopicFluentBuilder
    {
        IAwsConsumerTopicBuilderWithTopic WithConsumerName(string consumerName);
    }

    public interface IAwsConsumerTopicBuilderWithTopic
    {
        IAwsConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IAwsConsumerTopicBuilderWithQueues
    {
        IAwsConsumerTopicBuilderWithFifo WithSubscribedQueues(bool buildErrorQueues, params string[] queueNames);
    }
    
    public interface IAwsConsumerTopicBuilderWithFifo
    {
        IAwsConsumerTopicBuilder WithFifoSettings(bool isFifoQueue, bool isFifoContentBasedDeduplication);
    }
    
    public interface IAwsConsumerTopicBuilder
    {
        Task StartConsumingAsync<T>(string subscribeToQueueName, int numberOfInstances, int receiveMaximumNumberOfMessages, CancellationToken cancellationToken = default) where T : class, IHandler;
    }
}