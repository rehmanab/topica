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

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new SearchTriggeredMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "search.triggered.web.v1", Type = nameof(SearchTriggeredMessageV1) };

            await _producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, cancellationToken: stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
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
        
        await _producer1.ProduceBatchAsync(settings.WebAnalyticsTopicSettings.Source, messages, null, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, settings.WebAnalyticsTopicSettings.Source);

        return messages.Count;
    }
}