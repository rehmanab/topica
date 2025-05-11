using System.Threading;
using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IConsumerFluentBuilder
    {
        IConsumerBuilderWithTopic WithConsumerName(string consumerName);
    }

    public interface IConsumerBuilderWithTopic
    {
        IConsumerBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IConsumerBuilderWithQueues
    {
        IConsumerBuilder WithSubscribedQueues(params string[] queueNames);
    }
    
    public interface IConsumerBuilder
    {
        Task StartConsumingAsync<T>(CancellationToken cancellationToken) where T : class, IHandler;
    }
}