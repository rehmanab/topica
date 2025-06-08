using System.Threading.Tasks;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Topica.Aws.Consumers;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Aws.Producer;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Providers;

public class AwsQueueProvider(
    IAwsQueueService awsQueueService, 
    IAmazonSQS sqsClient, 
    IMessageHandlerExecutor messageHandlerExecutor, 
    ILogger<AwsQueueProvider> logger) : IQueueProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

    public async Task CreateQueueAsync(MessagingSettings settings)
    {
        var sqsConfiguration = new AwsSqsConfiguration
        {
            QueueAttributes = new AwsQueueAttributes
            {
                MessageVisibilityTimeout = settings.AwsMessageVisibilityTimeoutSeconds,
                IsFifoQueue = settings.AwsIsFifoQueue,
                IsFifoContentBasedDeduplication = settings.AwsIsFifoContentBasedDeduplication,
                QueueMaximumMessageSize = settings.AwsQueueMaximumMessageSizeKb,
                QueueMessageRetentionPeriodSeconds = settings.AwsQueueMessageRetentionPeriodSeconds,
                QueueMessageDelaySeconds = settings.AwsQueueMessageDelaySeconds,
                QueueReceiveMessageWaitTimeSeconds = settings.AwsQueueReceiveMessageWaitTimeSeconds
            },
            CreateErrorQueue = settings.AwsBuildWithErrorQueue,
            ErrorQueueMaxReceiveCount = settings.AwsErrorQueueMaxReceiveCount
        };
        
        _ = await awsQueueService.CreateQueueAsync(settings.Source, sqsConfiguration);
    }

    public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new AwsQueueConsumer(sqsClient, messageHandlerExecutor, awsQueueService, messagingSettings, logger);
    }

    public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;
        
        return new AwsQueueProducer(producerName, awsQueueService, sqsClient, messagingSettings.AwsIsFifoQueue);
    }
}