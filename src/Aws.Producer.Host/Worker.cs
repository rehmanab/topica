using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Aws.Producer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Aws.Producer.Host;

public class Worker(IAwsTopicService awsTopicService, IProducerBuilder producerBuilder, ProducerSettings producerSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer = await producerBuilder.BuildProducerAsync<IAmazonSimpleNotificationService>(null, producerSettings, stoppingToken);
        var topicArns = awsTopicService.GetAllTopics(producerSettings.Source, producerSettings.AwsIsFifoQueue).ToBlockingEnumerable(cancellationToken: stoppingToken).SelectMany(x => x).ToList();

        switch (topicArns.Count)
        {
            case 0:
                throw new Exception($"No topic found for prefix: {producerSettings?.Source}");
            case > 1:
                throw new Exception($"More than 1 topic found for prefix: {producerSettings?.Source}");
        }
        
        var topic = topicArns.First().TopicArn;
        
        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new OrderPlacedMessageV1{ConversationId = Guid.NewGuid(), OrderId = count, OrderName = Random.Shared.GenerateRandomMaleFirstAndLastName(), Type = nameof(OrderPlacedMessageV1)};
            var request = new PublishRequest
            {
                TopicArn = topic, 
                Message = JsonConvert.SerializeObject(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "SignatureVersion", new MessageAttributeValue { StringValue = "2", DataType = "String"} 
                    }
                }
            };
    
            if (topic.EndsWith(".fifo"))
            {
                request.MessageGroupId = Guid.NewGuid().ToString();
                request.MessageDeduplicationId = Guid.NewGuid().ToString();
            }
    
            await producer.PublishAsync(request, stoppingToken);
    
            count++;
    
            logger.LogInformation("Produced message to {ProducerSettingsSource}: {Count}", producerSettings?.Source, count);
    
            await Task.Delay(1000, stoppingToken);
        }

        producer.Dispose();

        logger.LogInformation("Finished: {Count} messages sent.", count);
    }
}