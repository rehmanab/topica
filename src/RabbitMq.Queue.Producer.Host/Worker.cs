using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Producer.Host.Settings;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;

namespace RabbitMq.Queue.Producer.Host;

public class Worker([FromKeyedServices("Producer")] IProducer producer, RabbitMqProducerSettings settings, ILogger<Worker> logger) : BackgroundService
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
            var message = new SearchTriggeredMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "search.triggered.web.v1", Type = nameof(SearchTriggeredMessageV1) };

            var attributes = new Dictionary<string, string>
            {
                // {"SignatureVersion", "2" },
                {"traceparent", "RMQ Queue" },
                {"tracestate", "RMQ Queue" },
            };
            
            await producer.ProduceAsync(message, attributes, cancellationToken: stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", producer.Source, $"{message.EventId} : {message.EventName}");
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
        
        await producer.ProduceBatchAsync(messages, attributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, producer.Source);

        return messages.Count;
    }
}