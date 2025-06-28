using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Kafka.Consumers;
using Topica.Kafka.Producers;
using Topica.Settings;

namespace Topica.Kafka.Providers;

public class KafkaTopicProvider(IMessageHandlerExecutor messageHandlerExecutor, ILogger<KafkaTopicProvider> logger) : ITopicProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;

    public async Task CreateTopicAsync(MessagingSettings settings, CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", settings.KafkaBootstrapServers) }).Build();
        
        try
        {
            var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            if (meta.Topics.Any(x => string.Equals(settings.Source, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
            {
                logger.LogInformation("**** EXISTS: topic {Source} already exists!", settings.Source);
            }
            else
            {
                await adminClient.CreateTopicsAsync([
                    new TopicSpecification { Name = settings.Source, ReplicationFactor = 1, NumPartitions = settings.KafkaNumberOfTopicPartitions }
                ]);

                logger.LogInformation("**** CREATED: Created topic {Source}", settings.Source);
            }
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "**** ERROR: {KafkaTopicProviderName}.{CreateTopicAsyncName}: An error occured creating topic {Topic}: {ErrorReason}", nameof(KafkaTopicProvider), nameof(CreateTopicAsync), ex.Results[0].Topic, ex.Results[0].Error.Reason);

            throw;
        }
    }

    public async Task<IConsumer> ProvideConsumerAsync( MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;
        
        return new KafkaTopicConsumer(messageHandlerExecutor, messagingSettings, logger);
    }

    public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;
        
        return new KafkaTopicProducer(producerName, messagingSettings);
    }
}