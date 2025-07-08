using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqTopicBuilder
    {
        IRabbitMqTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IRabbitMqTopicBuilderWithTopicName
    {
        IRabbitMqTopicBuilderWithQueueToSubscribeTo WithTopicName(string topicName);
    }
    
    public interface IRabbitMqTopicBuilderWithQueueToSubscribeTo
    {
        IRabbitMqTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IRabbitMqTopicBuilderWithBuild
    {
        IRabbitMqTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}