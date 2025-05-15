using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarConsumerTopicFluentBuilder
    {
        IPulsarConsumerTopicBuilderWithTopic WithConsumerName(string consumerName);
    }

    public interface IPulsarConsumerTopicBuilderWithTopic
    {
        IPulsarConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IPulsarConsumerTopicBuilderWithQueues
    {
        IPulsarConsumerTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup);
    }
    
    public interface IPulsarConsumerTopicBuilderWithConfiguration
    {
        IPulsarConsumerTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace);
    }
    
    public interface IPulsarConsumerTopicBuilderWithOptions
    {
        IPulsarConsumerTopicBuilder WithTopicOptions(bool startNewConsumerEarliest);
    }
    
    public interface IPulsarConsumerTopicBuilder
    {
        Task StartConsumingAsync<T>(int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler;
    }
}