using Kafka.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Host.Shared.Messages.V1;
using Topica.Kafka.Contracts;

namespace Kafka.Producer.Host;

public class Worker(IKafkaTopicFluentBuilder builder, KafkaProducerSettings settings, KafkaHostSettings hostSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithTopicSettings(settings.WebAnalyticsTopicSettings.StartFromEarliestMessages, settings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(hostSettings.BootstrapServers)
            .BuildProducerAsync(stoppingToken);
        
        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new CustomEventMessageV1 { EventId = count, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) };

            await producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        await producer1.DisposeAsync();
    }
}