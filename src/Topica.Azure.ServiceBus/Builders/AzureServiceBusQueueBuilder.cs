using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Builders;

public class AzureServiceBusQueueBuilder(IQueueProviderFactory queueProviderFactory)
    : IAzureServiceBusQueueBuilder, IAzureServiceBusQueueBuilderWithQueueName, IAzureServiceBusQueueBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private int? _numberOfInstances;

    public IAzureServiceBusQueueBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithNumberOfInstances(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.AzureServiceBus);
        var messagingSettings = GetMessagingSettings();

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.AzureServiceBus);

        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}