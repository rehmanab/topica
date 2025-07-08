using Kafka.Topic.Producer.Host.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;

namespace Kafka.Topic.Producer.Host;

public class Worker([FromKeyedServices("Producer")] IProducer producer, KafkaProducerSettings settings, KafkaHostSettings hostSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var count = await SendSingleAsync(stoppingToken);
        // var count = await SendBatchAsync(stoppingToken);

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
            
            await producer.ProduceAsync(message, attributes, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", producer.Source, count);
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

        await producer.ProduceBatchAsync(messages, attributes, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", producer.Source, messages.Count);

        return messages.Count;
    }
}