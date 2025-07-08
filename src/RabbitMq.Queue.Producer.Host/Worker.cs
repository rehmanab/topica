using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Producer.Host.Settings;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Queue.Producer.Host;

public class Worker(IRabbitMqQueueBuilder builder, RabbitMqProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder.BuildProducerAsync(stoppingToken);

        var count = await SendSingleAsync(stoppingToken);
        // var count = await SendBatchAsync(stoppingToken);

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new SearchTriggeredMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "search.triggered.web.v1", Type = nameof(SearchTriggeredMessageV1) };

            var attributes = new Dictionary<string, string>
            {
                // {"SignatureVersion", "2" },
                {"traceparent", "RMQ Queue" },
                {"tracestate", "RMQ Queue" },
            };
            
            await _producer1.ProduceAsync(message, attributes, cancellationToken: stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", _producer1.Source, $"{message.EventId} : {message.EventName}");
            count++;
            
            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 500)
            .Select(index => new SearchTriggeredMessageV1
            {
                ConversationId = Guid.NewGuid(), 
                EventId = index, 
                EventName = "search.triggered.web.v1", 
                Type = nameof(SearchTriggeredMessageV1)
            })
            .Cast<BaseMessage>()
            .ToList();
        
        var attributes = new Dictionary<string, string>
        {
            // {"SignatureVersion", "2" },
            {"traceparent", "RMQ Queue" },
            {"tracestate", "RMQ Queue" },
        };
        
        await _producer1.ProduceBatchAsync(messages, attributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, _producer1.Source);

        return messages.Count;
    }
}