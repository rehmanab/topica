using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqQueueCreationBuilder
    {
        IRabbitMqQueueBuilderWithQueueName WithWorkerName(string workerName);
    }

    public interface IRabbitMqQueueBuilderWithQueueName
    {
        IRabbitMqQueueBuilder WithQueueName(string queueName);
    }
    
    public interface IRabbitMqQueueBuilder
    {
        IRabbitMqQueueBuilder WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}