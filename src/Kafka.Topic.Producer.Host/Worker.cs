using Kafka.Topic.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Host.Shared.Messages.V1;
using Topica.Kafka.Contracts;
using Topica.Messages;

namespace Kafka.Topic.Producer.Host;

public class Worker(IKafkaTopicCreationBuilder builder, KafkaProducerSettings settings, KafkaHostSettings hostSettings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithTopicSettings(settings.WebAnalyticsTopicSettings.StartFromEarliestMessages, settings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(hostSettings.BootstrapServers)
            .BuildProducerAsync(stoppingToken);
        
        var count = await SendSingleAsync(stoppingToken);
        // var count = await SendBatchAsync(stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new CustomEventMessageV1 { EventId = count, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) };
            
            await _producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 500)
            .Select(index => new CustomEventMessageV1 { EventId = index, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) })
            .Cast<BaseMessage>()
            .ToList();

        await _producer1.ProduceBatchAsync(settings.WebAnalyticsTopicSettings.Source, messages, null, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, messages.Count);

        return messages.Count;
    }
}