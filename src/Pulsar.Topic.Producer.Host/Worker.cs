using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Topic.Producer.Host.Settings;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;
using Topica.Pulsar.Contracts;

namespace Pulsar.Topic.Producer.Host;

public class Worker(IPulsarTopicBuilder builder, PulsarProducerSettings settings, ILogger<Worker> logger) : BackgroundService
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
            var message = new FileDownloadedMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "file.downloaded.web.v1", Type = nameof(FileDownloadedMessageV1) };
            
            var applicationProperties = new Dictionary<string, string>
            {
                { "traceparent", "Pulsar" },
                { "tracestate", "Pulsar" }
            };
            
            await _producer1.ProduceAsync(message, applicationProperties, stoppingToken);
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", _producer1.Source, $"{message.EventId} : {message.EventName}");
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 5000)
            .Select(index => new FileDownloadedMessageV1 { ConversationId = Guid.NewGuid(), EventId = index, EventName = "file.downloaded.web.v1", Type = nameof(FileDownloadedMessageV1) })
            .Cast<BaseMessage>()
            .ToList();
        
        var applicationProperties = new Dictionary<string, string>
        {
            { "traceparent", "Pulsar" },
            { "tracestate", "Pulsar" }
        };

        await _producer1.ProduceBatchAsync(messages, applicationProperties, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", _producer1.Source, messages.Count);

        return messages.Count;
    }
}