using Kafka.Consumer.Host.Handlers.V1;
using Kafka.Consumer.Host.Messages.V1;
using Kafka.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Kafka.Contracts;

namespace Kafka.Consumer.Host;

public class Worker(IKafkaConsumerTopicFluentBuilder builder, KafkaConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await builder
            .WithConsumerName(nameof(PersonCreatedMessageV1))
            .WithTopicName(settings.PersonCreatedTopicSettings!.Source!)
            .WithConsumerGroup(settings.PersonCreatedTopicSettings!.ConsumerGroup!)
            .WithTopicSettings(settings.PersonCreatedTopicSettings.StartFromEarliestMessages, settings.PersonCreatedTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(settings.PersonCreatedTopicSettings!.BootstrapServers!)
            .StartConsumingAsync<PersonCreatedMessageHandlerV1>(
                    settings.PersonCreatedTopicSettings.NumberOfInstances, 
                    stoppingToken
            );
        
        await builder
            .WithConsumerName(nameof(PlaceCreatedMessageV1))
            .WithTopicName(settings.PlaceCreatedTopicSettings!.Source!)
            .WithConsumerGroup(settings.PlaceCreatedTopicSettings!.ConsumerGroup!)
            .WithTopicSettings(settings.PlaceCreatedTopicSettings.StartFromEarliestMessages, settings.PlaceCreatedTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(settings.PlaceCreatedTopicSettings!.BootstrapServers!)
            .StartConsumingAsync<PlaceCreatedMessageHandlerV1>(
                settings.PlaceCreatedTopicSettings.NumberOfInstances, 
                stoppingToken
            );
    }
}