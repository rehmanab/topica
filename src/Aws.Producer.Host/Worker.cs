using Aws.Producer.Host.Messages.V1;
using Aws.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;
using Topica.Aws.Contracts;

namespace Aws.Producer.Host;

public class Worker(IAwsTopicFluentBuilder builder, IAwsTopicService awsTopicService, AwsProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string workerName = $"{nameof(OrderPlacedMessageV1)}_aws_producer_host_1";
        
        var orderPlacedTopicProducer = await builder
            .WithWorkerName(workerName)
            .WithTopicName(settings.OrderPlacedTopicSettings.Source)
            .WithSubscribedQueues(
                settings.OrderPlacedTopicSettings.SubscribeToSource,
                settings.OrderPlacedTopicSettings.WithSubscribedQueues
            )
            .WithErrorQueueSettings(
                settings.OrderPlacedTopicSettings.BuildWithErrorQueue,
                settings.OrderPlacedTopicSettings.ErrorQueueMaxReceiveCount
            )
            .WithFifoSettings(
                settings.OrderPlacedTopicSettings.IsFifoQueue,
                settings.OrderPlacedTopicSettings.IsFifoContentBasedDeduplication
            )
            .WithTemporalSettings(
                settings.OrderPlacedTopicSettings.MessageVisibilityTimeoutSeconds,
                settings.OrderPlacedTopicSettings.QueueMessageDelaySeconds,
                settings.OrderPlacedTopicSettings.QueueMessageRetentionPeriodSeconds,
                settings.OrderPlacedTopicSettings.QueueReceiveMessageWaitTimeSeconds
            )
            .WithQueueSettings(settings.OrderPlacedTopicSettings.QueueMaximumMessageSize)
            .BuildProducerAsync(stoppingToken);
        
        var topicArns = awsTopicService.GetAllTopics(settings.OrderPlacedTopicSettings.Source, settings.OrderPlacedTopicSettings.IsFifoQueue).ToBlockingEnumerable(cancellationToken: stoppingToken).SelectMany(x => x).ToList();

        switch (topicArns.Count)
        {
            case 0:
                throw new Exception($"No topic found for prefix: {settings.OrderPlacedTopicSettings.Source}");
            case > 1:
                throw new Exception($"More than 1 topic found for prefix: {settings.OrderPlacedTopicSettings.Source}");
        }
        
        var topicArn = topicArns.First().TopicArn;
        
        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var messageGroupId = Guid.NewGuid().ToString();
            
            var message = new OrderPlacedMessageV1
            {
                ConversationId = Guid.NewGuid(), 
                OrderId = count, 
                OrderName = Random.Shared.GenerateRandomMaleFirstAndLastName(), 
                Type = nameof(OrderPlacedMessageV1),
                MessageGroupId = messageGroupId
            };

            var awsMessageAttributes = new Dictionary<string, string>
            {
                {"SignatureVersion", "2" }
            };
            await orderPlacedTopicProducer.ProduceAsync(topicArn, message, awsMessageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.OrderPlacedTopicSettings.Source, $"{message.OrderId} : {message.OrderName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        await orderPlacedTopicProducer.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
}