using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqQueueBuilder
    {
        IRabbitMqQueueBuilderWithQueueName WithWorkerName(string workerName);
    }

    public interface IRabbitMqQueueBuilderWithQueueName
    {
        IRabbitMqQueueBuilderWithBuild WithQueueName(string queueName);
    }
    
    public interface IRabbitMqQueueBuilderWithBuild
    {
        IRabbitMqQueueBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}