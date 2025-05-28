using Microsoft.Extensions.Hosting;
using Pulsar.Consumer.Host.Handlers.V1;
using Pulsar.Consumer.Host.Messages.V1;
using Pulsar.Consumer.Host.Settings;
using Topica.Pulsar.Contracts;

namespace Pulsar.Consumer.Host;

public class Worker(IPulsarConsumerTopicFluentBuilder builder, PulsarConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(nameof(DataSentMessageV1))
                .WithTopicName(settings.DataSentTopicSettings.Source)
                .WithConsumerGroup(settings.DataSentTopicSettings.ConsumerGroup)
                .WithConfiguration(
                    settings.DataSentTopicSettings.Tenant,
                    settings.DataSentTopicSettings.Namespace,
                    settings.DataSentTopicSettings.NumberOfPartitions
                )
                .WithTopicOptions(settings.DataSentTopicSettings.StartNewConsumerEarliest)
                .BuildConsumerAsync(
                    settings.DataSentTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<DataSentMessageHandlerV1>(stoppingToken);

        await (await builder
                .WithWorkerName(nameof(MatchStartedMessageV1))
                .WithTopicName(settings.MatchStartedTopicSettings.Source)
                .WithConsumerGroup(settings.MatchStartedTopicSettings.ConsumerGroup)
                .WithConfiguration(
                    settings.MatchStartedTopicSettings.Tenant,
                    settings.MatchStartedTopicSettings.Namespace,
                    settings.MatchStartedTopicSettings.NumberOfPartitions
                )
                .WithTopicOptions(settings.MatchStartedTopicSettings.StartNewConsumerEarliest)
                .BuildConsumerAsync(
                    settings.MatchStartedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<MatchStartedMessageHandlerV1>(stoppingToken);
    }
}