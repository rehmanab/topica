using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqQueueBuilder(
    IQueueProviderFactory queueProviderFactory) 
    : IRabbitMqQueueBuilder, IRabbitMqQueueBuilderWithQueueName, IRabbitMqQueueBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private int? _numberOfInstances;

    public IRabbitMqQueueBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqQueueBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IRabbitMqQueueBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();
        
        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            SubscribeToSource = _queueName,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}