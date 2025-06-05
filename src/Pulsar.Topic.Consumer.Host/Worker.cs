using Microsoft.Extensions.Hosting;
using Pulsar.Topic.Consumer.Host.Settings;
using Topica.Pulsar.Contracts;

namespace Pulsar.Topic.Consumer.Host;

public class Worker(IPulsarTopicCreationBuilder builder, PulsarConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
                .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
                .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
                .WithConfiguration(
                    settings.WebAnalyticsTopicSettings.Tenant,
                    settings.WebAnalyticsTopicSettings.Namespace,
                    settings.WebAnalyticsTopicSettings.NumberOfPartitions
                )
                .WithTopicOptions(settings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
                .BuildConsumerAsync(
                    settings.WebAnalyticsTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync(stoppingToken);
    }
}