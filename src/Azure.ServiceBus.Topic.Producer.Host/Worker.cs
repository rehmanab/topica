using Azure.ServiceBus.Topic.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;

namespace Azure.ServiceBus.Topic.Producer.Host;

public class Worker(IAzureServiceBusTopicBuilder builder, AzureServiceBusProducerSettings settings, ILogger<Worker> logger) : BackgroundService
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
            var message = new VideoPlayedMessageV1
            {
                EventId = count,
                EventName = "video.played.web.v1",
                ConversationId = Guid.NewGuid(),
                Type = nameof(VideoPlayedMessageV1),
                RaisingComponent = settings.WebAnalyticsTopicSettings.WorkerName,
                Version = "V1",
                MessageAdditionalProperties = new Dictionary<string, string> { { "key", "val" } }
            };

            var applicationProperties = new Dictionary<string, string> { { "traceparent", "SB" }, { "tracestate", "SB" } };
            
            await _producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, applicationProperties, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, count);
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
                RaisingComponent = settings.WebAnalyticsTopicSettings.WorkerName,
                Version = "V1",
                MessageAdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
            })
            .Cast<BaseMessage>()
            .ToList();

        var applicationProperties = new Dictionary<string, string> { { "traceparent", "SB" }, { "tracestate", "SB" } };

        await _producer1.ProduceBatchAsync(settings.WebAnalyticsTopicSettings.Source, messages, applicationProperties, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, messages.Count);

        return messages.Count;
    }
}