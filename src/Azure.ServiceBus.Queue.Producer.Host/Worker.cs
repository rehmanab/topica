using Azure.ServiceBus.Queue.Producer.Host.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Messages;
using Topica.SharedMessageHandlers.Messages.V1;

namespace Azure.ServiceBus.Queue.Producer.Host;

public class Worker([FromKeyedServices("Producer")] IProducer producer, AzureServiceBusProducerSettings settings, ILogger<Worker> logger) : BackgroundService
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
            var message = new VideoPlayedMessageV1
            {
                EventId = count,
                EventName = "video.played.web.v1",
                ConversationId = Guid.NewGuid(),
                Type = nameof(VideoPlayedMessageV1),
                RaisingComponent = settings.WebAnalyticsQueueSettings.WorkerName,
                Version = "V1",
                MessageAdditionalProperties = new Dictionary<string, string> { { "key", "val" } }
            };

            var applicationProperties = new Dictionary<string, string> { { "traceparent", "SB" }, { "tracestate", "SB" } };
            
            await producer.ProduceAsync(message, applicationProperties, stoppingToken);
            logger.LogInformation("Sent to {Queue}: {Count}", producer.Source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 50)
            .Select(index => new VideoPlayedMessageV1
            {
                EventId = index,
                EventName = "video.played.web.v1",
                ConversationId = Guid.NewGuid(),
                Type = nameof(VideoPlayedMessageV1),
                RaisingComponent = settings.WebAnalyticsQueueSettings.WorkerName,
                Version = "V1",
                MessageAdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
            })
            .Cast<BaseMessage>()
            .ToList();

        var applicationProperties = new Dictionary<string, string> { { "traceparent", "SB" }, { "tracestate", "SB" } };

        await producer.ProduceBatchAsync(messages, applicationProperties, stoppingToken);
        logger.LogInformation("Sent batch to {Queue}: {Count}", producer.Source, messages.Count);

        return messages.Count;
    }
}