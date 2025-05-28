using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Producer.Host.Messages.V1;
using Pulsar.Producer.Host.Settings;
using RandomNameGeneratorLibrary;
using Topica.Pulsar.Contracts;

namespace Pulsar.Producer.Host;

public class Worker(IPulsarConsumerTopicFluentBuilder builder, PulsarProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string workerName = $"{nameof(DataSentMessageV1)}_pulsar_producer_host_1";

        var dataSentProducer = await builder
            .WithWorkerName(workerName)
            .WithTopicName(settings.DataSentTopicSettings.Source)
            .WithConsumerGroup(settings.DataSentTopicSettings.ConsumerGroup)
            .WithConfiguration(
                settings.DataSentTopicSettings.Tenant,
                settings.DataSentTopicSettings.Namespace,
                settings.DataSentTopicSettings.NumberOfPartitions
            )
            .WithTopicOptions(settings.DataSentTopicSettings.StartNewConsumerEarliest)
            .WithProducerOptions(
                settings.DataSentTopicSettings.BlockIfQueueFull,
                settings.DataSentTopicSettings.MaxPendingMessages,
                settings.DataSentTopicSettings.MaxPendingMessagesAcrossPartitions,
                settings.DataSentTopicSettings.EnableBatching,
                settings.DataSentTopicSettings.EnableChunking,
                settings.DataSentTopicSettings.BatchingMaxMessages,
                settings.DataSentTopicSettings.BatchingMaxPublishDelayMilliseconds
            )
            .BuildProducerAsync(stoppingToken);

        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new DataSentMessageV1 { ConversationId = Guid.NewGuid(), DataId = count, DataName = Random.Shared.GenerateRandomMaleFirstAndLastName(), Type = nameof(DataSentMessageV1) };

            await dataSentProducer.ProduceAsync(settings.DataSentTopicSettings.Source, message, null, stoppingToken);
            
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.DataSentTopicSettings.Source, $"{message.DataId} : {message.DataName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        await dataSentProducer.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
}