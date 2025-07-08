using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.RabbitMq.Contracts
{
    public interface IRabbitMqQueueCreationBuilder
    {
        IRabbitMqQueueCreationBuilderWithQueueName WithWorkerName(string workerName);
    }

    public interface IRabbitMqQueueCreationBuilderWithQueueName
    {
        IRabbitMqQueueCreationBuilderWithBuild WithQueueName(string queueName);
    }
    
    public interface IRabbitMqQueueCreationBuilderWithBuild
    {
        IRabbitMqQueueCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}