using Kafka.Topic.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Kafka.Contracts;

namespace Kafka.Topic.Consumer.Host;

public class Worker(IKafkaTopicCreationBuilder builder, KafkaConsumerSettings settings, KafkaHostSettings hostSettings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithTopicSettings(settings.WebAnalyticsTopicSettings.StartFromEarliestMessages, settings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(hostSettings.BootstrapServers)
            .BuildConsumerAsync(
                settings.WebAnalyticsTopicSettings.NumberOfInstances,
                stoppingToken
            );

        await consumer1.ConsumeAsync(stoppingToken);
    }
}