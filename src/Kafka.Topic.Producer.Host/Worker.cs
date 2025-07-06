using Kafka.Topic.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Kafka.Contracts;
using Topica.Messages;

namespace Kafka.Topic.Producer.Host;

public class Worker(IKafkaTopicBuilder builder, KafkaProducerSettings settings, KafkaHostSettings hostSettings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder.BuildProducerAsync(stoppingToken);
        
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
            
            var attributes = new Dictionary<string, string>
            {
                // {"SignatureVersion", "2" },
                {"traceparent", "kafka" },
                {"tracestate", "kafka" },
            };
            
            await _producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, attributes, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var attributes = new Dictionary<string, string>
        {
            // {"SignatureVersion", "2" },
            {"traceparent", "kafka" },
            {"tracestate", "kafka" },
        };
        
        var messages = Enumerable.Range(1, 500)
            .Select(index => new CustomEventMessageV1 { EventId = index, EventName = "custom.event.web.v1", ConversationId = Guid.NewGuid(), Type = nameof(CustomEventMessageV1) })
            .Cast<BaseMessage>()
            .ToList();

        await _producer1.ProduceBatchAsync(settings.WebAnalyticsTopicSettings.Source, messages, attributes, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, messages.Count);

        return messages.Count;
    }
}