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
        IRabbitMqQueueBuilderWithBuild WithQueueName(string queueName);
    }
    
    public interface IRabbitMqQueueBuilderWithBuild
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}