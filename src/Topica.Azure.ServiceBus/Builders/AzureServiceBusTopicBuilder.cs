using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Builders;

public class AzureServiceBusTopicBuilder(ITopicProviderFactory topicProviderFactory)
    : IAzureServiceBusTopicBuilder, IAzureServiceBusTopicBuilderWithTopicName, IAzureServiceBusTopicBuilderWithSubscribeToSubscription, IAzureServiceBusTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private int? _numberOfInstances;
    private string _subscribeToSubscription = null!;

    public IAzureServiceBusTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithSubscribeToSubscription WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithSubscribeToSubscription(string subscribeToSubscription)
    {
        _subscribeToSubscription = subscribeToSubscription;
        return this;
    }

    public IAzureServiceBusTopicBuilderWithBuild WithNumberOfInstances(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.AzureServiceBus);
        var messagingSettings = GetMessagingSettings();

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.AzureServiceBus);

        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            SubscribeToSource = _subscribeToSubscription,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}