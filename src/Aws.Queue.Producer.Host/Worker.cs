using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;

namespace Aws.Queue.Producer.Host;

public class Worker([FromKeyedServices("Producer")] IProducer producer, ILogger<Worker> logger) : BackgroundService
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
            var messageGroupId = Guid.NewGuid().ToString();
            
            var message = new ButtonClickedMessageV1
            {
                ConversationId = Guid.NewGuid(), 
                EventId = count, 
                EventName = "button.clicked.web.v1", 
                Type = nameof(ButtonClickedMessageV1),
                MessageGroupId = messageGroupId
            };

            var attributes = new Dictionary<string, string>
            {
                // {"SignatureVersion", "2" },
                {"traceparent", "AWS queue" },
                {"tracestate", "AWS queue" },
            };
            
            await producer.ProduceAsync(message, attributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", producer.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string queueName, CancellationToken stoppingToken)
    {
        var messageGroupId = Guid.NewGuid();
            
        var messages = Enumerable.Range(1, 500)
            .Select(index => new ButtonClickedMessageV1
            {
                ConversationId = Guid.NewGuid(),
                EventId = index,
                EventName = "button.clicked.web.v1",
                Type = nameof(ButtonClickedMessageV1),
                MessageGroupId = messageGroupId.ToString()
            })
            .Cast<BaseMessage>()
            .ToList();

        var attributes = new Dictionary<string, string>
        {
            // {"SignatureVersion", "2" },
            {"traceparent", "AWS queue" },
            {"tracestate", "AWS queue" },
        };
        
        await producer.ProduceBatchAsync(messages, attributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, producer.Source);

        return messages.Count;
    }
}