using Kafka.Consumer.Host.Handlers.V1;
using Kafka.Consumer.Host.Messages.V1;
using Kafka.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Kafka.Contracts;

namespace Kafka.Consumer.Host;

public class Worker(IKafkaTopicFluentBuilder builder, KafkaConsumerSettings settings, KafkaHostSettings hostSettings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(nameof(PersonCreatedMessageV1))
                .WithTopicName(settings.PersonCreatedTopicSettings.Source)
                .WithConsumerGroup(settings.PersonCreatedTopicSettings.ConsumerGroup)
                .WithTopicSettings(settings.PersonCreatedTopicSettings.StartFromEarliestMessages, settings.PersonCreatedTopicSettings.NumberOfTopicPartitions)
                .WithBootstrapServers(hostSettings.BootstrapServers)
                .BuildConsumerAsync(
                    settings.PersonCreatedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<PersonCreatedMessageHandlerV1>(stoppingToken);

        await (await builder
                .WithWorkerName(nameof(PlaceCreatedMessageV1))
                .WithTopicName(settings.PlaceCreatedTopicSettings.Source)
                .WithConsumerGroup(settings.PlaceCreatedTopicSettings.ConsumerGroup)
                .WithTopicSettings(settings.PlaceCreatedTopicSettings.StartFromEarliestMessages, settings.PlaceCreatedTopicSettings.NumberOfTopicPartitions)
                .WithBootstrapServers(hostSettings.BootstrapServers)
                .BuildConsumerAsync(
                    settings.PlaceCreatedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<PlaceCreatedMessageHandlerV1>(stoppingToken);
    }
}