using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aws.Topic.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;
using Topica.Messages;

namespace Aws.Topic.Producer.Host;

public class Worker(IAwsTopicBuilder builder, AwsProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder.BuildProducerAsync(stoppingToken);
        
        var count = await SendSingleAsync(settings.WebAnalyticsTopicSettings.Source, stoppingToken);
        // var count = await SendBatchAsync(settings.WebAnalyticsTopicSettings.Source, stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(string topicName, CancellationToken stoppingToken)
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

            var messageAttributes = new Dictionary<string, string>
            {
                {"SignatureVersion", "2" }
            };
            
            await _producer1.ProduceAsync(topicName, message, messageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", TopicQueueHelper.AddTopicQueueNameFifoSuffix(settings.WebAnalyticsTopicSettings.Source, settings.WebAnalyticsTopicSettings.IsFifoQueue ?? false), $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string topicName, CancellationToken stoppingToken)
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

        var messageAttributes = new Dictionary<string, string>
        {
            {"SignatureVersion", "2" }
        };
        
        await _producer1.ProduceBatchAsync(topicName, messages, messageAttributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, TopicQueueHelper.AddTopicQueueNameFifoSuffix(settings.WebAnalyticsTopicSettings.Source, settings.WebAnalyticsTopicSettings.IsFifoQueue ?? false));

        return messages.Count;
    }
}